using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MicHandle : MonoBehaviour
{
    private string _device;
    AudioClip _clipRecord = null;
    AudioSource audi;
    public float FreqCalibrater = 10f;
    float lowest_value_clamp = -120;
    float highest_value_clamp = 50;
    //mic initialization
    void Start()
    {
        for (int i = 0; i < lvl_sliders.Length; i++)
        {
            max_indicators[i].minValue = lvl_sliders[i].minValue = lowest_value_clamp;
            max_indicators[i].maxValue = lvl_sliders[i].maxValue = highest_value_clamp;          
            lvl_sliders[i].wholeNumbers = true;
            CurrentMaxlvl[i] = lowest_value_clamp;

        }
        audi = gameObject.GetComponent<AudioSource>();
        InitMic();       
    }
    void InitMic()
    {
       
        int restart_capture = 500; 
        if (_device == null) _device = Microphone.devices[0];
        _clipRecord = Microphone.Start(_device, true, restart_capture, AudioSettings.outputSampleRate);
        audi.clip = _clipRecord;
        audi.Play();
        Invoke("ResetMic", restart_capture);
    }
    void ResetMic()
    {
        StopMicrophone();
        //rather reload scene here.
        InitMic();
    }

    public float lvl;
    float[] amples = new float[512];
    public Slider[] lvl_sliders = new Slider[14];
    public Slider[] max_indicators = new Slider[14];
    public TextMeshProUGUI[] text_indic = new TextMeshProUGUI[14];
    public float[] LowFreqSamples = new float[14];
    public float[] CurrentMaxlvl = new float[14];
    public int next_freq_set;
    public TextMeshProUGUI which_set;
    public float Tier1;
    public float Tier2;
    float maxlvl;
    public float SilenceThreshold = -50;
    bool resetGuard;
    void LateUpdate()
    {
        AnalyzeSound();
        for(int i = 0; i < LowFreqSamples.Length; i++)
        {
            float lowfreqabs = LowFreqSamples[i] = (20 * Mathf.Log10(Mathf.Abs(amples[i]))) + FreqCalibrater;
            lvl_sliders[i].value = lowfreqabs;

           
            if (lowfreqabs > CurrentMaxlvl[i])
            {
                resetGuard = true;
                max_indicators[i].value = CurrentMaxlvl[i] = lowfreqabs;
                maxlvl = Mathf.Round(CurrentMaxlvl[i]);
                text_indic[i].text = maxlvl.ToString();             
            }
            if (maxlvl > -20)
            {
                Tier1++;
               
                dBTranslator();
               // reset_single_max_value(i);
                Debug.Log("high rate RECORDED");
            }
            else { TextDisplay("def"); }
            if (maxlvl >= -10)
            {
                Tier2++;
                dBTranslator();
                Debug.Log("highest rate RECORDED!");
            }
            if(lowfreqabs < SilenceThreshold && resetGuard)
            {
                reset_single_max_value(i);
                Tier1 = 0;
                Tier2 = 0;
                resetGuard = false;
            }
        }


    }
    //testing values used for calibration
    public float delttier1 = 4;
    public float deltier2 = 3;
    public float deltiermax = 4;
    void dBTranslator()
    {
        if (Tier1 >= delttier1)
        {
            TextDisplay("600");

        }
        else if (Tier1 <= delttier1)
        {
            TextDisplay("def");
        }
        if (Tier2 >= deltiermax)
        {
            TextDisplay("1200");
          
        }
        else if(Tier2 >= deltier2)
        {
            TextDisplay("900");
        }
   
        //if() most frequencies are more than -20dB, then denote a 600ml/sec value
        //if() 2 - 5 freq bands are showing signal strength > -10dB denote a 900ml/sec value
        //if() more than 4 freq bands show strength > -10 dB and if same freq bands from 900ml band indicate any gain, denote 1200ml/sec value
    }
  
    void TextDisplay(string arg)
    {
        switch(arg)
        {
            case ("def"):
                Invoke("delayedtxt_erase", 4f);
                break;
            case ("600"):
                which_set.text = "600 ml/sec";
                reset_max_values();
                //Tier1 = 0;
                break;
            case ("900"):
                which_set.text = "900 ml/sec";
                //Tier2 = 0;
                break;
            case ("1200"):
                which_set.text = "1200 ml/sec";
                break;
        }
    }
    void delayedtxt_erase()
    {
        which_set.text = "0 ml/sec";

    }

    void AnalyzeSound()
    {
        audi.GetSpectrumData(amples, 0, FFTWindow.Blackman);
    }

    public void reset_max_values()
    {
        for(int i = 0; i < CurrentMaxlvl.Length; i++)
        {
            max_indicators[i].value = CurrentMaxlvl[i] = lowest_value_clamp;
        }
    }
    public void reset_single_max_value(int i)
    {
        max_indicators[i].value = CurrentMaxlvl[i] = lowest_value_clamp;
    }


    public TextMeshProUGUI tier1disp;
    public TextMeshProUGUI tier2disp;
    public TextMeshProUGUI tier3disp;
    public void tier1button(bool param)
    {
        if (param)
        {
            delttier1++;
        }
        else if (!param)
        {
            delttier1--;
        }
        tier1disp.text = delttier1.ToString();
    }
    public void tier2button(bool param)
    {
        if (param)
        {
            deltier2++;
        }
        else if (!param)
        {
            deltier2--;
        }
        tier2disp.text = deltier2.ToString();
    }
    public void tier3button(bool param)
    {
        if (param)
        {
            deltiermax++;
        }
        else if (!param)
        {
            deltiermax--;
        }
        tier3disp.text = deltiermax.ToString();
    }

    void StopMicrophone()
    {
        Microphone.End(_device);
    }
    bool _isInitialized;
    // start mic when scene starts
    void OnEnable()
    {
        //InitMic();
        _isInitialized = true;
    }
    //stop mic when loading a new level or quit application
    void OnDisable()
    {
        StopMicrophone();
    }
    void OnDestroy()
    {
        StopMicrophone();
    }
    private void OnApplicationFocus(bool focus)
    {
       if(focus)
       {
            ResetMic();
       }
        else { return; }
    }

}






