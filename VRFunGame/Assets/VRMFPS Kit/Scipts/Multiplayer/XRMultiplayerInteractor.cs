using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{    
    /// <summary>
    /// Script that makes sure that all player owned interactors claim network authority
    /// over network grabbables that are grabbed
    /// </summary>
    public class XRMultiplayerInteractor : NetworkBehaviour
    {
        public static XRMultiplayerInteractor localInstance;
        
        #region Client Authority
        
        /// <summary>
        /// Method that properly claims authority of a Grabbable Object to our client
        /// </summary>
        /// <param name="grabbedIdentity">Identity containing XRGrabInteractable</param>
        [Client]
        public void NetworkClaimGrabbable(NetworkIdentity grabbedIdentity)
        {
            //Null check
            if(grabbedIdentity == null) return;
            
            //no need to claim any net id if we already own it
            if (grabbedIdentity.isOwned) return;
            
            //If netId is found, claim authority of it
            CMD_ClaimClientAuthority(grabbedIdentity);
            
            //Claim authority of every interactor on object (like sockets)
            foreach (var objectInteractor in grabbedIdentity.GetComponentsInChildren<XRBaseInteractor>())
                TransferInteractorAuthority(objectInteractor, true);
            
            //If object has grab Interactable, fix rigidbody kinematic state
            XRGrabInteractable grabInteractable = grabbedIdentity.GetComponentInParent<XRGrabInteractable>();
            if (grabInteractable)
                FixClientGrabInteractableKinematicState(grabInteractable);
        }

        /// <summary>
        /// Method that properly resign authority of a Grabbable Object to our client
        /// </summary>
        /// <param name="releasedIdentity">Identity containing XRGrabInteractable</param>
        [Client]
        public void NetworkReleaseGrabbable(NetworkIdentity releasedIdentity)
        {
            //Null check
            if(releasedIdentity == null) return;
            
            //If netId is found, return authority of it to server
            CMD_ReturnAuthorityToServer(releasedIdentity);
            
            //Resign authority of child interactors (like sockets) of identity
            foreach (var childInteractor in releasedIdentity.GetComponentsInChildren<XRBaseInteractor>())
                TransferInteractorAuthority(childInteractor, false);
            
            //If there is a rigidbody on identity, transfer properties (like velocity) to server
            Rigidbody rb = releasedIdentity.GetComponent<Rigidbody>();
            
            if (!rb) return;
            
            CMD_TransferRigidbodyToServer(releasedIdentity, rb.velocity);
        }

        /// <summary>
        /// Problem: when we grab an object (on client), XRGrabInteractable registers rigidbody as
        /// being kinematic since NetworkRigidbody component needs rigidbody to be kinematic
        /// whenever we dont own it (on the client). This results in things like throwing objects
        /// Not working since XRGrabInteractable thinks the rigidbody always has been kinematic
        ///    
        /// Solution: Using reflection, we change the "wasKinematic" bool of the XRGrabInteractable
        /// whenever we grab it to always be false*/
        /// </summary>
        /// <param name="grabInteractable"></param>
        [Client]
        private void FixClientGrabInteractableKinematicState(XRGrabInteractable grabInteractable)
        {
            FieldInfo wasKinematicField = typeof(XRGrabInteractable).GetField("m_WasKinematic", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            //Ensure the wasKinematic field was found
            if (wasKinematicField == null)
            {
                Debug.LogError("Field m_WasKinematic could not be found through reflection on " +
                               "XRGrabInteractable. Are you using a newer version of the XR Interaction Toolkit?" +
                               "This will likely result in grabables staying kinematic after releasing them.");
                return;
            }
            
            //Override the kinematic state, that was wrongly set by NetworkRigidbody component
            //not yet being owned, with the proper wasKinematic state
            wasKinematicField.SetValue(grabInteractable, false);
            //TODO ensure kinematic objects stay kinematic after release, there is a private wasKinematic server synced on NetworkRigidbody component which we can use
        }
        #endregion
        
        #region Network Calls
        /// <summary>
        /// Allows client to claim any network identity. (Potentially unsafe!),
        /// write claim safety checks here if that is important.
        /// </summary>
        /// <param name="identityToClaim">The net id that the player is going to claim</param>
        [Command]
        private void CMD_ClaimClientAuthority(NetworkIdentity identityToClaim)
        {
            //Reclaim any potential authority from previous clients
            identityToClaim.RemoveClientAuthority();
            //Give authority to client who owns the player
            identityToClaim.AssignClientAuthority(connectionToClient);

            //Change sync direction of identity components on server
            ChangeSyncDirection(identityToClaim, SyncDirection.ClientToServer);
            //Notify all clients that all NetworkBehaviours now has client authority
            RPC_ChangeSyncDirection(identityToClaim, SyncDirection.ClientToServer);
        }
        
        [Command]
        private void CMD_ReturnAuthorityToServer(NetworkIdentity identity)
        {
            //Remove any clients authority
            identity.RemoveClientAuthority();

            //Change sync direction of identity components on server
            //we need this to take effect immediately
            ChangeSyncDirection(identity, SyncDirection.ServerToClient);
            
            //Notify all clients that all NetworkBehaviours now has server authority again
            RPC_ChangeSyncDirection(identity, SyncDirection.ServerToClient);
        }

        [Command]
        private void CMD_TransferRigidbodyToServer(NetworkIdentity identity, Vector3 clientVelocity)
        {
            Rigidbody rb = identity.GetComponent<Rigidbody>();

            if (!rb)
                return;
            
            rb.isKinematic = false;
            rb.velocity = clientVelocity;
        }

        /// <summary>
        /// Useful when an interaction is forcibly ended on a client, and we need the same to happen on the server.
        /// </summary>
        /// <param name="interactableIdentity"></param>
        [Command]
        public void CMD_ForceDetachInteractable(NetworkIdentity interactableIdentity)
        {
            XRBaseInteractable interactable = interactableIdentity.GetComponent<XRBaseInteractable>();
            
            interactable.interactionManager.CancelInteractableSelection((IXRSelectInteractable)interactable);
        }
        
        [ClientRpc]
        private void RPC_ChangeSyncDirection(NetworkIdentity identity, SyncDirection newDirection)
        {
            //Change sync direction on all clients
            ChangeSyncDirection(identity, newDirection);
        }
        
        private void ChangeSyncDirection(NetworkIdentity identity, SyncDirection newDirection)
        {
            //Change sync direction of all net behaviours on identity
            //like NetworkTransforms, NetworkRigidbodies, and player made net scripts
            foreach (var netBehaviour in identity.GetComponents<NetworkBehaviour>())
            {
                netBehaviour.syncDirection = newDirection;
            }
        }

        #endregion
        
        #region Interactor Ownership
        /// <summary>
        /// Prepares/Unprepares an interactor for authority behaviour.
        /// Claim all currently held interactables, subscribe to transfer events etc
        /// </summary>
        /// <param name="interactor"></param>
        /// <param name="clientAuthority"></param>
        private void TransferInteractorAuthority(XRBaseInteractor interactor, bool clientAuthority)
        {
            //Transfer selected interactable authority
            foreach (var selectedInteractable in interactor.interactablesSelected)
            {
                NetworkIdentity identity = selectedInteractable.transform.GetComponent<NetworkIdentity>();
                if(!identity) continue;
                
                if(clientAuthority)
                    NetworkClaimGrabbable(identity);
                else 
                    NetworkReleaseGrabbable(identity);
            }
            
            //Subscribe/Unsubscribe to interactor events
            if (clientAuthority)
            {
                //Listen for select events (when we grab/release objects)
                interactor.selectEntered.AddListener(InteractorSelectEvent);
                interactor.selectExited.AddListener(InteractorExitEvent);
            }
            else
            {
                interactor.selectEntered.RemoveListener(InteractorSelectEvent);
                interactor.selectExited.RemoveListener(InteractorExitEvent);
            }
        }
        #endregion

        #region Grab Event Handling
        
        /// <summary>
        /// Called whenever one of the local players interactors grabs anything
        /// </summary>
        /// <param name="args">event arguments</param>
        [Client]
        private void InteractorSelectEvent(SelectEnterEventArgs args)
        {
            //Try to find a netId component on the grabbed object, or its parents
            NetworkIdentity grabbedIdentity = args.interactableObject.transform.GetComponentInParent<NetworkIdentity>();
            
            //If no net id was found, we can't claim it
            if(grabbedIdentity == null) return;

            NetworkClaimGrabbable(grabbedIdentity);
        }

        /// <summary>
        /// Called whenever one of the local players interactors lets go of anything
        /// </summary>
        /// <param name="args">event arguments</param>
        [Client]
        private async void InteractorExitEvent(SelectExitEventArgs args)
        {
            XRBaseInteractable releasedInteractable = args.interactableObject as XRBaseInteractable;
            
            //Null check
            if (releasedInteractable == null) return;
            

            //Try to find a network identity component on interactable object or its parents
            NetworkIdentity releasedIdentity = releasedInteractable.GetComponentInParent<NetworkIdentity>();
        
            //If no net id was found, it isn't a network object.
            //There isn't any authority to resign
            if(releasedIdentity == null) return;

            
            //If another interactable is found on the parent,
            //that means the main interactable hasn't yet been released.
            //Our client authority should not be resigned yet if a child object is released
            if (releasedInteractable.transform.parent != null)
            {
                //GetComponentInParent includes components on self, hence why we start from our parent and search upwards
                XRGrabInteractable parentInteractable =
                    releasedInteractable.transform.parent.GetComponentInParent<XRGrabInteractable>();
                
                //A parent interactable was found, return
                if (parentInteractable) return;
            }
            

            //wait and see if it was selected the next coming frames
            await Task.Delay(20);
            //Don't resign authority if releasedInteractable was just transferred to another one of our interactors
            //Example: when transferring interactable from left hand to right hand
            if (releasedInteractable.isSelected)
            {
                NetworkIdentity newInteractorIdentity = releasedInteractable.interactorsSelecting[0].transform.GetComponentInParent<NetworkIdentity>();

                //If object was transferred one of our other owned interactors, no need to resign client authority
                if (newInteractorIdentity && newInteractorIdentity.isOwned) return;
            }
            
            
            //Wait until next frame, so throw has began before we release on network
            await Task.Delay(1);
            
            //If all checks pass, begin network release
            NetworkReleaseGrabbable(releasedIdentity);
        }
        #endregion
        
        // Start is called before the first frame update
        void Start()
        {
            SocketInteractorKeepSelectTargetValidWarning();

            if (isOwned)
            {
                localInstance = this;
                
                //Make sure all interactors on player are initialized (Register events, claim child interactables etc)
                foreach (var playerInteractor in GetComponentsInChildren<XRBaseInteractor>())
                    TransferInteractorAuthority(playerInteractor, true);
            }
        }

        /// <summary>
        /// Takes in account whether behaviour is client or server authoritative to determine our authority status.
        /// Use this primarily when you need to determine authority on code that can be executed on both server & client
        /// </summary>
        /// <param name="netBehaviour">The Network Behaviour Component you want to determine authority of</param>
        /// <returns>Has authority</returns>
        public static bool HasBehaviourAuthority(NetworkBehaviour netBehaviour)
        {
            if (netBehaviour.syncDirection == SyncDirection.ServerToClient) return netBehaviour.isServer;
            if (netBehaviour.syncDirection == SyncDirection.ClientToServer) return netBehaviour.isOwned;

            return false;
        }
        
        private void SocketInteractorKeepSelectTargetValidWarning()
        {
            foreach (var sceneSocketInteractor in FindObjectsByType<XRSocketInteractor>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (!sceneSocketInteractor.keepSelectedTargetValid) continue;
                
                Debug.LogWarning($"XRSocketInteractor component on object '{sceneSocketInteractor.gameObject.name}' " +
                                 "has keepSelectedTargetValid enabled, this will lead to position tearing if multiple interactors on different clients" +
                                 "try to select the same object. Highly recommend turning this off unless you know what you are doing!");
            }
        }
    }
}