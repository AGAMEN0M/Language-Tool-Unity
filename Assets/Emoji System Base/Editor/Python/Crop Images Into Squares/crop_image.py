import os
from PIL import Image

def cut_image_into_squares(image_png, square_size, destination_folder):
    img = Image.open(image_png)
    width, height = img.size

    # Calculates the number of squares horizontally and vertically.
    number_squares_horizontal = width // square_size
    number_squares_vertical = height // square_size

    # Create destination folder if it doesn't exist.
    if not os.path.exists(destination_folder):
        os.makedirs(destination_folder)

    for j in range(number_squares_vertical):
        for i in range(number_squares_horizontal):
            # Calculates the coordinates of the square.
            left = i * square_size
            top = j * square_size
            right = left + square_size
            bottom = top + square_size

            # Crop and save the square as a new image in the destination folder.
            square = img.crop((left, top, right, bottom))
            file_name = f'square_{j}_{i}.png'
            destination_path = os.path.join(destination_folder, file_name)
            square.save(destination_path)

# Prompts user for image path.
image_path = input("Enter the image path: ")

# Checks if the image path is valid.
while not os.path.isfile(image_path):
    print("Invalid image path. Try again.")
    image_path = input("Enter the image path: ")

# Prompts the user for the size of the square.
square_size = int(input("Enter the size of the square: "))

# Prompts user for destination folder name.
destination_folder = input("Enter the destination folder name: ")

# Call the function to crop the image.
cut_image_into_squares(image_path, square_size, destination_folder)
