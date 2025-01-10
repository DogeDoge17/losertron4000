from genericpath import isfile
import os
import os.path
import json

outp = ""

resPath = "Resources\\Raw"

hideBase = len(resPath)

def recursiveWalk(path):
    files = os.listdir(path)
    
    splitt = path.split("\\")    

    name = splitt[len(splitt)-1]
    foundFiles = []
    foundDirs = []

    for file in files:
        if os.path.isfile(os.path.join(path, file)):
            foundFiles.append(file)
        else:
            foundDirs.append(recursiveWalk(os.path.join(path, file)))

    return  {"name": name , "fullPath": path[hideBase:],"dirs": foundDirs ,"files": foundFiles }

with open(os.path.join(resPath, "map.json"), 'w') as file:
    json.dump(recursiveWalk(resPath), file, indent=4)

print("finished mapping")

'''for root, dirs, files in os.walk(resPath):
    for dir in dirs: 
        relative_path = os.path.relpath(os.path.join(root, dir), resPath)
        outp += relative_path + "\n"
    for file in files:
        relative_path = os.path.relpath(os.path.join(root, file), resPath)
        outp += relative_path + "\n"'''

#print(recursiveWalk(resPath))



#with open(resPath + '\\map.txt', 'w') as f:
#	f.write(outp)




