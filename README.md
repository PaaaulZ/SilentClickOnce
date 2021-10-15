# SilentClickOnce
Install/Uninstall ClickOnce without prompting the user

## Need to install or uninstall a ClickOnce .application without prompting the user? Maybe you want to manage your application using your DC and you can't because ClickOnce needs the user to press "Install/Uninstall"? SilentClickOnce is what you need.

Microsoft supports installing ClickOnce .application files silently by using a custom installer as noted in [the Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019).
Well, this is just a custom installer ready to use, nothing special.

## How can I use it?

Simply compile this, call it from a command line and pipe the output to a file to see what's happening.

Install example: **SilentClickOnce.exe -i "\\\\192.168.1.2\\apps\\MyApp\\MyApp.application" > MyApp.log**
Uninstall example: **SilentClickOnce.exe -u MyApp > MyApp.log**

Don't want to compile or can't? Download the release [here on GitHub](https://github.com/PaaaulZ/SilentClickOnce/releases/) and download the ready to use file.

## Why is this special?

This is nothing special. I made this for me to use and made it public because people around the internet keep saying that you can't silently install a ClickOnce. Someone even said that only malware installs silently completely forgetting that maybe you want to push an internal use application to every client in your company domain.
If you stumble on someone saying you can't I hope you'll end up here and solve your problem with a quick and simple solution.


For every information check on [the Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019). 

If you don't trust this or you want to make it yourself check [the Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019), the code is almost the same.
