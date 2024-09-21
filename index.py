import os
from PIL import Image
import colorsys

# List of color transformations with hue, saturation (%), and brightness adjustment (percentage)
colors = [
    ("White", 0, 0.0, 0),
    ("Black", 0, 0.0, -84), 
    ("Gray", 0, 0.0, -64),
    ("Red", 0, 0.6, -40),  
    ("Green", 120, 0.30, -60),
    ("Olive", 78, 0.20, -45),
    ("Blue", 208, 0.61, -55),
    ("Navy", 208, 0.45, -65),
    ("Pink", 306, 0.40, 0),
    ("Purple", 295, 0.25, -50),
]

# Function to adjust hue, saturation, and brightness (as percentage) while preserving transparency
def adjust_hue_saturation_brightness(image, hue_shift, saturation_scale, brightness_percent):
    img = image.convert('RGBA')  # Keep alpha channel intact
    pixels = img.load()

    for y in range(img.height):
        for x in range(img.width):
            r, g, b, a = pixels[x, y]  # Get RGBA values

            # Skip processing if the pixel is fully transparent
            if a == 0:
                continue

            # Convert RGB to HLS (Hue, Lightness, Saturation)
            h, l, s = colorsys.rgb_to_hls(r / 255.0, g / 255.0, b / 255.0)

            # Directly adjust hue based on the desired shift in degrees, normalized
            h = (hue_shift / 360.0) % 1.0

            # Adjust saturation (scaling based on provided value)
            s = saturation_scale

            # Adjust brightness by a percentage
            brightness_adjust = brightness_percent / 100.0
            l = max(0, min(1, l * (1 + brightness_adjust)))  # Scale the brightness by percentage

            # Convert back to RGB
            r, g, b = colorsys.hls_to_rgb(h, l, s)
            pixels[x, y] = int(r * 255), int(g * 255), int(b * 255), a  # Preserve the original alpha

    return img

def process_images(input_folder):
    # Process each PNG file in the input folder
    for root, dirs, files in os.walk(input_folder):
        for filename in files: 
            if filename.endswith(".png"):
                img_path = os.path.join(root, filename)
                img = Image.open(img_path)

                # Get the base filename without the extension
                base_name = os.path.splitext(filename)[0]

                # Determine if the image is in the shirts or pants folder
                folder_name = os.path.basename(root)
                parent_folder = os.path.basename(os.path.dirname(root))

                if parent_folder == 'Shirts':
                    output_filename = "Shirt.png"
                elif parent_folder == 'Pants':
                    output_filename = "Pants.png"
                else:
                    continue  # Skip if not in the expected folders
                
                # Skip if the item is called Overlay.png
                if base_name.lower() == 'overlay':
                    continue

                # Load overlay if it exists, checking one folder deeper
                overlay_path = os.path.join(root, "overlay.png")
                overlay_img = None
                if os.path.exists(overlay_path):
                    overlay_img = Image.open(overlay_path).convert('RGBA')

                # Apply all color transformations
                for color_name, hue_shift, saturation_scale, brightness_percent in colors:
                    new_img = adjust_hue_saturation_brightness(img, hue_shift, saturation_scale, brightness_percent)

                    # If an overlay exists, composite it onto the transformed image
                    if overlay_img:
                        new_img = Image.alpha_composite(new_img, overlay_img)

                    # Create output folder using the naming convention
                    itemname = f"{base_name}_{color_name}"
                    output_location = os.path.join(output_folder, parent_folder, folder_name, itemname)
                    os.makedirs(output_location, exist_ok=True)

                    # Save the transformed image in the output folder
                    output_file_path = os.path.join(output_location, output_filename)
                    new_img.save(output_file_path)
                    print(f"Generated: {output_file_path}")

# Example usage
input_folder = "input" 
output_folder = "output"
process_images(input_folder)
