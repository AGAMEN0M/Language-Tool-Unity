import sys
import os
from PIL import Image

def cut_image_into_squares(image_png, square_size, destination_folder):
    # Open the image file.
    img = Image.open(image_png)
    width, height = img.size

    # Calculate the number of squares that can fit horizontally and vertically.
    number_squares_horizontal = width // square_size
    number_squares_vertical = height // square_size

    # Create the destination folder if it doesn't exist.
    if not os.path.exists(destination_folder):
        os.makedirs(destination_folder)

    # Loop through the number of squares vertically.
    for j in range(number_squares_vertical):
        # Loop through the number of squares horizontally.
        for i in range(number_squares_horizontal):
            # Calculate the coordinates of the current square.
            left = i * square_size
            top = j * square_size
            right = left + square_size
            bottom = top + square_size

            # Crop the image to get the square.
            square = img.crop((left, top, right, bottom))
            # Create a filename for the cropped square.
            file_name = f'square_{j}_{i}.png'
            # Create the full path to save the cropped square.
            destination_path = os.path.join(destination_folder, file_name)
            # Save the cropped square to the destination folder.
            square.save(destination_path)

    # Print a success message.
    print("Images successfully applied to folder.")

if __name__ == "__main__":
    # Check if the correct number of command-line arguments is provided.
    if len(sys.argv) != 4:
        print("Usage: script.py <image_path> <square_size> <destination_folder>")
        sys.exit(1)

    # Retrieve command-line arguments.
    image_path = sys.argv[1]
    square_size = int(sys.argv[2])
    destination_folder = sys.argv[3]

    # Call the function to cut the image into squares.
    cut_image_into_squares(image_path, square_size, destination_folder)
