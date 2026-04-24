from gtts import gTTS
import os
from playsound3 import playsound
import datetime

path = "audio/"

if not os.path.exists(path):
    os.makedirs(path)

async def generate_speech(text: str, language: str = 'es', accent: str = 'com.mx'):
    myobj = gTTS(text=text, lang=language, slow=False, tld=accent)  # tld='com.mx' for Mexican Spanish accent

    filename = f"voice_{datetime.datetime.now().strftime('%Y-%m-%d_%H-%M-%S')}.mp3"
    file_path = os.path.join(path, filename)
    myobj.save(file_path)
    return filename

def play_audio(filename: str):
    # playsound(path + filename)
    pass

async def list_audio_files():
    return [f for f in os.listdir(path) if f.endswith('.mp3')]

async def delete_audio_file(filename: str):
    file_path = os.path.join(path, filename)
    if os.path.exists(file_path):
        os.remove(file_path)
        return True
    return False

async def delete_all_audio_files():
    for filename in os.listdir(path):
        if filename.endswith('.mp3'):
            os.remove(os.path.join(path, filename))

async def get_audio_file_path(filename: str):
    file_path = os.path.join(path, filename)
    if os.path.exists(file_path):
        return file_path
    return None

async def download_audio_file(filename: str):
    file_path = os.path.join(path, filename)
    if os.path.exists(file_path):
        return file_path
    return None