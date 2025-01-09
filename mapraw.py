import os

outp = ""

resPath = "Resources\\Raw"

for root, dirs, files in os.walk(resPath):
    for dir in dirs: 
        relative_path = os.path.relpath(os.path.join(root, dir), resPath)
        outp += relative_path + "\n"
    for file in files:
        relative_path = os.path.relpath(os.path.join(root, file), resPath)
        outp += relative_path + "\n"

with open(resPath + '\\map.txt', 'w') as f:
	f.write(outp)

<<<<<<< Updated upstream
print("success")
=======
print("finished mapping")
>>>>>>> Stashed changes
