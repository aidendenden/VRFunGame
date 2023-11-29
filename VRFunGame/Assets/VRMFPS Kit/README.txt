Thank you so much for purchasing our package!

You can find the project documentation page at: 
https://scuttled-tech.gitbook.io/vrfpskit-docs/

If you ever need direct support, feel free to reach out over at our
Discord Server: https://discord.gg/KbFKA5Z6Sr
We'd love if you'd share your games there as well!


BEFORE YOU START
- Make sure your OpenXR input device is configured in Project Settings


MULTIPLAYER TROUBLESHOOTING
- By default, port 630 is used (you can change this as you wish in Multiplayer Manager).
- Make sure port 630 UDP&TCP ingoing is open in Windows Firewall
- Make sure you have Port Forwarded port 630 to your pc in your Router, so your server is accessible to players outside your network
- If you still can't connect on Windows, make sure your network type is set to Private (and not Public)


HOW TO SET UP YOUR OWN GRABBABLE OBJECTS
- Set up a GameObject with XRGrabInteractable, Rigidbidy, and a collider
- If you want it to be synced over network, you must have the "NetworkIdentity" component
- Use components like "NetworkTransform" & "NetworkRigidbody" to sync position, velocity etc
- You can read more on Mirror networking over at: https://mirror-networking.gitbook.io/docs/
- Interactor Authority is handled automatically through the XRMultiplayerInteractor component on the player. 
  Client Authority is returned to the server when you release an object
  Authority is also handled on nested interactors (like the scope mount sockets, that let weapons grab weapon attachments)
