# Huey

A very simple script to automatically generate color variations of clothing, primarily meant for Unturned.

## How to use

Using this is fairly simple, as long as you understand a little bit of Python.

### Getting started
Make sure you have python installed.

1. Install Pillow:
   
   `pip install pillow`

2. Put your PNG's in the input folder as shown in the repo.

3. Run the application:
   
   `python index.py`

4. ???
5. Profit.

### Using it

As you may have noticed, within the input folder you have multiple files and folders.
You can add multiple files in any folder, however it is limited by type and by depth.

#### Different colors

You can always add or remove colors, the colors are modified using the Hue, Saturation and Brightness modifiers set at the top of the script.

#### Overlay

If you want a part of an image not to be changed from color you can make an `Overlay.png`, an `Overlay.png` will be overlayed on any PNG file within the directory.
This will allow you to easily add shoes, such as the example for the shorts.

#### Shirt or Pants?

You can make both, just put the png file in the appropriate location within the folder structure.