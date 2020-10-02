# VoIPainter

## Description

VoIPainter is a simple Windows utility designed to change the background on a user's Cisco VoIP phone, without requiring access to the CUCM backend.

## **Requirements**

- The phone must be owned by the executing user in CUCM.
- Personalization must be enabled on the phone in CUCM.
- The phone must have web access enabled in CUCM (the commands are sent to it via HTTP).
- The executing user must have *at least* local administrative privileges; VoIPainter will open a socket on TCP port 80, which requires administrative rights.

## Use
1. Open VoIPainter.
1. Use the "Browse For Image" button to navigate to and select the desired image. Keep in mind that uncluttered, lower contrast images work best.
1. Find your phone's IP address.[^1] Enter the address in the "Phone IP" field.
[^1]: This can typically be acquired from the phone's settings menu under "Phone Information", "Status", or similar. It should be in the form `0.0.0.0`. 
1. Enter your domain username (e.g. Timothy Elmer -> telmer) in the "Username" field.
1. Enter your domain password in the "Password" field.
1. Select your phone model from the "Phone Model" dropdown. If you are unsure, the model is typically labeled on the back/bottom of the phone.
1. Click "Apply".
