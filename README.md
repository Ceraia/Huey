# Huey

A very simple script to automatically generate color variations of clothing, primarily meant for Unturned.

## Table of Contens
- [Huey](#huey)
  - [Table of Contens](#table-of-contens)
  - [1. How to use](#1-how-to-use)
    - [1.1. Getting started](#11-getting-started)
    - [1.2. Using it](#12-using-it)
      - [1.2.1. Different colors](#121-different-colors)
      - [1.2.2. Overlay](#122-overlay)
      - [1.2.3. Shirt or Pants?](#123-shirt-or-pants)

## 1. How to use

Using this is fairly simple, as long as you understand a little bit of Python.

### 1.1. Getting started
Make sure you have python installed.

1. Install Pillow:
   
   `pip install pillow`

2. Put your PNG's in the input folder as shown in the repo.

3. Run the application:
   
   `python index.py`

4. ???
5. Profit.

### 1.2. Using it

As you may have noticed, within the input folder you have multiple files and folders.
You can add multiple files in any folder, however it is limited by type and by depth.

#### 1.2.1. Different colors

You can always add or remove colors, the colors are modified using the Hue, Saturation and Brightness modifiers set at the top of the script.

#### 1.2.2. Overlay

If you want a part of an image not to be changed from color you can make an `Overlay.png`, an `Overlay.png` will be overlayed on any PNG file within the directory.
This will allow you to easily add shoes, such as the example for the shorts.

#### 1.2.3. Shirt or Pants?

You can make both, just put the png file in the appropriate location within the folder structure.