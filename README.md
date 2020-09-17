# SilentClickOnce
Install ClickOnce without prompting the user or showing anything

## Need to install a ClickOnce .application without prompting the user? Maybe you want to push your application using your DC and you can't because ClickOnce needs the user to press "Install"? SilentClickOnce is what you need.

Microsoft supports installing ClickOnce .application files silently by using a custom installer as noted in [the Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019).
Well, this is just a custom installer ready to use, nothing special.

## How can I use it?

Simply compile this, call it from a command line and pipe the output to a file to see what's happening.
Ex: **SilentClickOnce.exe \\\\192.168.1.2\\apps\\MyApp\\MyApp.application > MyApp.log**

Unfortunately I can't upload releases and I don't feel good making random people on the internet download an exe from my website hosted on a free web hosting service, sounds sketchy don't you think?

## Why is this special?

This is nothing special. I made this for me to use and made it public because people around the internet keep saying that you can't silently install a ClickOnce. Someone even said that only malware installs silently completely forgetting that maybe you want to push an internal use application to every client in your company domain.
If you stumble on someone saying you can't I hope you'll end up here and solve your problem with a quick and simple solution.


For every information check on [the Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019). 

If you don't trust this or you want to make it yourself check [the Microsoft Docs](https://docs.microsoft.com/en-us/visualstudio/deployment/walkthrough-creating-a-custom-installer-for-a-clickonce-application?view=vs-2019), the code is almost the same.
