# VRCLovenseConnect
VRCLovenseConnect is a .NET implementation of the Lovense Connect API and Buttplug.io to synchronize **any connected toy** with a VRChat avatar through OSC messages.

## What does it do exactly?
With the power of **OSC** and **Avatar Dynamics**, your avatar sends a value between 0.0 and 1.0 to this program through UDP, according to the distance between a Contact Sender and the center of the Contact Receiver.

This value is converted and transferred to the Lovense Connect through HTTP, with a call to vibrate a connected toy.

It has been tested to be **accurate**, **lightweight** and **fast**. OSC is so fast indeed that I had to set a limit of processed messages to have less delay lol.

## Requirements
Lovense Connect for Android or iOS (also works on PC but requires the Lovense Bluetooth dongle).

# Toy Setup
There are currently two protocols available for VRCLovenseConnect: "Lovense" and "Buttplug".

## Buttplug.io (NEW!)
Simply connect your toy to your PC through a cable or Bluetooth. Lovense toys should work with basic features in this protocol too.

You can test the connectivity of your toys with Intiface Desktop.

## Lovense Connect
Open the Lovense Connect app on your phone or PC, then select the green shield icon to reveal a URL.

In the config.json file next to the executable, copy this URL to the "address" field.

Then simply launch the program after VRChat is open.

## config.json
Next to the executable should be a config file named "config.json". Open it with any text editor to change its values.

**oscPort**: The port to listen OSC messages on. Default is 9001.

**protocol**: The protocol to use for toy controls (Lovense or Buttplug).

**address**: Only for Lovense protocol. The address provided by the Lovense Connect app on your phone (not Lovense Remote).

**scanTime**: Only for Buttplug protocol. The time to scan for a toy in seconds. Default is 5.

**limit**: Only for debugging. The number of received messages to skip for better performances. Default is 10.

**commandAll**: Allows all commands to be activated just by the Avatar Parameter for vibration, or with separate parameters.

**vibrateParameter**: The Avatar Parameter to synchronize with for vibration commands.

**pumpParameter**: The Avatar Parameter to synchronize with for pumping/linear commands (unused if commandAll = false).

**rotateParameter**: The Avatar Parameter to synchronize with for rotation commands (unused if commandAll = false).

# Avatar Setup
Your avatar requires a spherical Contact Receiver with "Proximity" mode. Set it up to intereact with any Contact Sender you like, whether it's a standard (hands, head...) or a custom one (see "Recommended Contacts Setup" in the DPS section further below).

This Contact Receiver has to be "Local Only" and generate a parameter with the same name as in the config.json file ("LovenseHaptics" by default). Reminder that parameters are case-sensitive.

Finally, add a float in your avatar's Expression Parameters with the same name, default to zero and no saving.

## Dynamic Penetration System
### Contact Setup
If you use Dynamic Penetration System by Raliv, for penetrators, Contacts should be at the base, and the radius should have the same length as set in the material settings. Since DPS detection is also spherical, this setup will have 1:1 accuracy.

For orifices that you want to set up with VRCLovenseConnect, you can place Contacts on "Orifice" objects in your avatar, set the center as deep inside the orifices as you want, and adjust the radius to be right at the entrance. Remember, Contact Receivers have to be spherical with "Proximity" on.

### Recommended Contacts Setup
For a penetrator:
- one Contact Receiver with at least the "Hands" standard tag, and "Allow Self" and "Allow Others" enabled.
- one Contact Receiver with an "Orifice" custom tag, and "Allow Others" enabled only.
- one Contact Sender with a "Penetrator" tag.
- (Optional) one Contact Receiver with an "OrificeSelf" custom tag, and "Allow Self" enabled only.
- (Optional) one Contact Sender with a "PenetratorSelf" tag.

For each orifice on which you want toy interactions enabled:
- one Contact Receiver with at least the "Fingers" standard tag, and "Allow Self" and "Allow Others" enabled.
- one Contact Receiver with a "Penetrator" custom tag, and "Allow Others" enabled only.
- one Contact Sender with an "Orifice" tag.
- (Optional) one Contact Receiver with a "PenetratorSelf" custom tag, and "Allow Self" enabled only.
- (Optional) one Contact Sender with an "OrificeSelf" tag.

This setup will make sure that you and others can control your toys without any weird behavior. Tags can be changed to be shared only to a few people for a more private use, kind of like a password.

## Documentation
VRChat OSC: https://docs.vrchat.com/docs/osc-overview

VRChat Contacts: https://docs.vrchat.com/v2022.1.2/docs/contacts

Dynamic Penetration System: https://raliv.gumroad.com/l/lwthuB

Intiface Desktop: https://intiface.com/desktop/

Buttplug.io C#: https://github.com/buttplugio/buttplug-rs-ffi/tree/master/csharp

LovenseConnectCSharp: https://github.com/MistressPlague/LovenseConnectCSharp
