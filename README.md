# Huey (v2)

A utility script to produce color variations of clothing within Unity for Unturned.

# Huey License (Based on AGPL-3.0)

**Preamble**

This license is based on the GNU Affero General Public License, version 3 (AGPL-3.0). It grants users the freedom to use, modify, and distribute the software, provided that modifications and improvements are shared with the community under the same terms. Additionally, any derivative work, service, or software that incorporates this project, *Huey*, must acknowledge its use of the project.

## Terms and Conditions for Copying, Distribution, and Modification

**0. Definitions**

This License applies to any program or other work that contains a notice placed by the copyright holder stating it is licensed under the terms of this *Huey License*.

**1. Additional Requirement: Acknowledgment of Use**

In addition to the conditions set forth in AGPL-3.0, the following terms apply:

- Any work, product, or service that includes or is derived from the Huey project, or any modified version of it, must include a clear and visible acknowledgment in the documentation, source code, or user interface stating: “This project uses Huey, originally developed by Jasper Visser.”
  
- The acknowledgment must be retained in any modified versions and be accessible to users or viewers of the derived work in a prominent place.

**2. Remaining License Terms**

All other terms of the AGPL-3.0 apply to this software. You must comply with the AGPL-3.0 in all respects, with the additional acknowledgment requirement listed above.

---

## 1. Table of Contents

- [Huey (v2)](#huey-v2)
- [Huey License (Based on AGPL-3.0)](#huey-license-based-on-agpl-30)
  - [Terms and Conditions for Copying, Distribution, and Modification](#terms-and-conditions-for-copying-distribution-and-modification)
  - [1. Table of Contents](#1-table-of-contents)
  - [2. How to install](#2-how-to-install)
    - [2.1. Unity Package File](#21-unity-package-file)
    - [2.2. Manually Moving Huey Scripts](#22-manually-moving-huey-scripts)
  - [3. How to use](#3-how-to-use)
    - [3.1. Using textures](#31-using-textures)
    - [3.2. Different colors](#32-different-colors)
    - [3.3. Overlay](#33-overlay)

## 2. How to install

Using this has become a lot more simple than the old v1 version.

### 2.1. Unity Package File

Easiest is to import the unity package file and then you are done.

### 2.2. Manually Moving Huey Scripts

You can move the Huey folder in this repository into the root of your project, and then everything should work swell.

## 3. How to use

Using it has become a lot simpler, simply right click anywhere in your unity editor, and if everything has been installed correctly you will be able to see an option for `Huey`, and then select `Process Clothing`.

![Right click menu option showcase](image.png)

### 3.1. Using textures

You can add any (preferably white) texture and the script will automatically create color variations.

### 3.2. Different colors

You can always add or remove colors, the colors are modified using the Hue, Saturation and Brightness modifiers set in the ColorSettings file.

### 3.3. Overlay

If you want a part of an image not to be changed from color you can make an Overlay.png, an Overlay.png will be overlayed on any PNG file within the directory. This will allow you to easily add shoes, such as the example for the shorts.
