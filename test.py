import os
from PIL import Image
import colorsys

# List of color transformations with hue, saturation (%), and brightness adjustment (percentage)
colors = [
    ("white", 0, 0.0, 0),
    ("black", 0, 0.0, -84), 
    ("gray", 0, 0.0, -64),
    ("red", 0, 0.6, -40),  
    ("green", 120, 0.30, -60),
    ("olive", 78, 0.20, -45),
    ("blue", 208, 0.61, -55),
    ("navy", 208, 0.45, -65),
    ("pink", 306, 0.40, 0),
    ("purple", 295, 0.25, -50),
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
            h = hue_shift / 360.0

            # Adjust saturation (scaling based on provided value)
            s = saturation_scale

            # Adjust brightness by a percentage
            brightness_adjust = brightness_percent / 100.0
            l = max(0, min(1, l * (1 + brightness_adjust)))  # Scale the brightness by percentage

            # Convert back to RGB
            r, g, b = colorsys.hls_to_rgb(h, l, s)
            pixels[x, y] = int(r * 255), int(g * 255), int(b * 255), a  # Preserve the original alpha

    return img

def process_images(input_folder, output_folder):
    # Ensure output folder exists
    if not os.path.exists(output_folder):
        os.makedirs(output_folder)

    # Process each PNG file in the input folder
    for filename in os.listdir(input_folder):
        if filename.endswith(".png"):
            img_path = os.path.join(input_folder, filename)
            img = Image.open(img_path)

            # Apply all color transformations
            for color_name, hue_shift, saturation_scale, brightness_percent in colors:
                new_img = adjust_hue_saturation_brightness(img, hue_shift, saturation_scale, brightness_percent)

                # Save the transformed image in the output folder with a descriptive name
                output_filename = f"{os.path.splitext(filename)[0]}_{color_name}.png"
                new_img.save(os.path.join(output_folder, output_filename))
                print(f"Generated: {output_filename}")

# Example usage
input_folder = "input_folder_path"  # Replace with your input folder path
output_folder = "output_folder_path"  # Replace with your output folder path
process_images(input_folder, output_folder)
