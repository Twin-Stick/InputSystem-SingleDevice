# Overview
Demonstration of how to lock a device to a user with the <B>InputSystem</B> and <B>PlayerInput</B> component.  
This is aimed at a single player game and targeting console, but will work just as well for PC.

# How to Use
- Open the <B>SampleScene</B> and enter play mode.
- You will be greeted with the player engagement screen, press <B>A</B> button on gamepad or <B>SPACE</B> on keyboard.
- You can move the sprite with <B>left thumbstick</B> for gamepad or <B>WADS</B> on keyboard.

# Expected Behaviour
This example will respond to device lost events in the case a gamepad becomes disconnected.  
It will then wait for a reconnection (eg; replace batteries), or add a new gamepad and associate to the <B>InputUser</B>.
It will also unpair all devices from the <B>InputUser</B> so during gameplay, input is only sent from the main paired device.

# Feedback and Changes
Please by all means add to this example and enhance the functionality!
