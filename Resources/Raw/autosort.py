import os
import sys
import shutil

if(len(sys.argv) < 1):
	print("please include path of where the images should be sampled from")

filesrc = sys.argv[1]

files = os.listdir(filesrc)

print("making dirs")

if not os.path.isdir(os.getcwd() + "\\eyes"):
	os.makedirs(os.getcwd() + "\\eyes") 
if not os.path.isdir(os.getcwd() + "\\body"):
	os.makedirs(os.getcwd() + "\\body") 
if not os.path.isdir(os.getcwd() + "\\brows"):
	os.makedirs(os.getcwd() + "\\brows") 
if not os.path.isdir(os.getcwd() + "\\head"):
	os.makedirs(os.getcwd() + "\\head") 
if not os.path.isdir(os.getcwd() + "\\mouth"):
	os.makedirs(os.getcwd() + "\\mouth")
if not os.path.isdir(os.getcwd() + "\\nose"):
	os.makedirs(os.getcwd() + "\\nose") 
if not os.path.isdir(os.getcwd() + "\\extra"):
	os.makedirs(os.getcwd() + "\\extra") 

print("sorting")
for file in files:
	if not file.endswith(".png"): 
		continue

	if "eyes_" in file :
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\eyes\\" + file)
		print("put " + file + " into eyes")	
	elif "eyebrows_" in file:
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\brows\\" + file)
		print("put " + file + " into brows")
	elif "face" in file and "scream" not in file:
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\head\\" + file)
		print("put " + file + " into face")
	elif "mouth_" in file:
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\mouth\\" + file)
		print("put " + file + " into mouth")
	elif "nose_" in file:
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\nose\\" + file)
		print("put " + file + " into nose")
	elif ("crossed" in file or  "turned_" in file or "shy_" in file or "forward_" in file or "lean_" in file) and "scream" not in file:
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\body\\" + file)
		print("put " + file + " into body")
	else:
		shutil.copy(filesrc + "\\" + file, os.getcwd() + "\\extra\\" + file)
		print("put " + file + " into extra")

print("finished successfully")

