using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    public int width;
    public int height;

    public bool fullscreen;
    public bool borderlessWindowed;

    public int sound;

    public int refreshRate;

    public Settings(){
        
    }

    public Settings(int w, int h, bool full, bool borderless, int refreshRate){
        width = w;
        height= h;
        fullscreen = full;
        borderlessWindowed = borderless;
        sound = 100;
        this.refreshRate = refreshRate;
    }

    public Settings(int w, int h, bool full, bool borderless,int refreshRate, int sound){
        width = w;
        height= h;
        fullscreen = full;
        borderlessWindowed = borderless;
        this.refreshRate = refreshRate;
        if(sound <= 100 || sound >= 0){
            this.sound = sound;
        }
        else{
            sound = 100;
        }
    }
}
