# Chaos Conductor

## Welcome

This repository is home to Chaos Conductor, a mod and toolkit for making custom The Murder of Sonic the Hedgehog (TMoStH) stories! This project begun as a way for myself and a friend to write our own tomfoolery into TMoStH and make the characters say funny things, but quickly ballooned into a fully-fledged toolkit that I decided to polish and share for public use.

Chaos Conductor is not a text replacement tool for TMoStH, but rather a mod that lets you write your own stories into the game. This mod allows you to use all of the existing assets (character portraits, backgrounds, SFX and music) to remix and create your own gripping narrative, as well as grant the ability to insert your own assets as simple audio and image files that doesn't require any sort of custom pipeline or processing - just drop your stuff in a folder and it can be loaded in-game!

Think of Chaos Conductor like a toolkit to make Visual Novel stories, but using the TMoStH engine and pre-loaded with official Sonic the Hedgehog assets.

## Install

First off - **get the game!** The Murder of Sonic the Hedgehog is [available and free to obtain on Steam](https://store.steampowered.com/app/2324650/The_Murder_of_Sonic_the_Hedgehog/) as of writing this Readme. You'll need to boot the game up at least once in order to have the game fully ready to go.

In order to install Chaos Conductor, head to the [Releases](../../releases) and download the latest one. Follow the instructions provided in a handy "install.txt" text file (it goes into more detail, in case you feel lost!).

To quickly summarize the installation process (you can skip this section if you intend to follow the install guide, which you should do): you will need to take the files from the included .zip and place them in your TMoStH install directory. From there, the mod will be enabled -- look for the version number on the bottom right hand corner of the main menu to ensure the mod loaded correctly. From there, you'll take the included "customs" folder and place it in your AppData folder for TMoStH (LocalLow/Sonic Social/The Murder of Sonic the Hedgehog).

Once you've placed the mod files in your game's install and the "customs" folder in the game's AppData folder, you should be able to see the game load with a new "Customs" main menu option, and one template story available to play and dive more into!

## Development

If you simply want to create custom stories using Chaos Conductor, you will not need to follow any of this! You should refer instead to the [Wiki](../../wiki) attached to this repository. However, if you wish to modify the source code for this mod, then keep reading.

All of the source code for this mod was written in C#, using [BepInEx](https://github.com/BepInEx/BepInEx) and [HarmonyX](https://github.com/BepInEx/HarmonyX). I work on this mod in Visual Studio 2022, using a few batch scripts for quickly compiling and copying the code back into the environment as well as the game for rapid testing. VS2022 is not required to work on this mod, but is recommended as it is an overall robust editor for this kind of project. What is required, however, is the .NET SDK which comes bundled with the .NET CLI necessary for compiling this mod.

There are some aspects to the full development environment that I unfortunately cannot provide here due to copyright reasons. Despite TMoStH being a freely available game, it is unlawful to distribute its contents (you still have to agree to a EULA when you obtain the game!). However, you yourself are fully able to decompile and rebuild a Unity editor environment from your own install of TMoStH using various tools! I recommend a tool named [AssetRipper](https://github.com/AssetRipper/AssetRipper) to perform this task.

Remember: *a full recreation of the game's editor environment is not totally necessary!* Source code changes and DLL builds will only need to link to the existing code for the game, which can simply be pulled down and referenced in your own environment (a guide for setting this up will be available in the [Wiki](../../wiki)!). Having the environment loaded in the Unity editor may help, but is not required at all (and in some cases may even slow you down!)

Additionally, a custom AssetBundle containing some runtime-loaded files and prefabs has been included with this repository, and contains only custom additions (again, copyright reasons - all aspects of this repository are 100% made by me!). If you wish to add to or modify this AssetBundle, a rebuild of the game's editor environment will be necessary!

## Credits

Created by GDFauxtrot (Twit/X: [@gdfauxtrot](https://twitter.com/gdfauxtrot))

Some testing done by Snaggypeets (Twit/X: [@snaggypeets](https://twitter.com/snaggypeets))

Shout outs to [DEEGEEMIN](https://twitter.com/deegeemin), lead character artist for TMoStH, for indulging me to talk about this mod and being excited for me to finish it while meeting at Sonic Revolution! That was a morale boost, and I would've left this project 80% completed and unpublished if not for them.