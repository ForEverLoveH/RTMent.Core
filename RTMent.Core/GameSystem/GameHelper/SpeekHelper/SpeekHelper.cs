using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RTMent.Core.GameSystem.GameHelper 
{
    public class SpeekHelper:Singleton<SpeekHelper>
    {
        private Queue<string> _speakers = new Queue<string>();
        bool IsSpeeking=false;
        Thread Thread;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void AddDataToQueue(string data)
        {
            _speakers.Enqueue(data);
            if(_speakers.Count > 0 )
            {
                lock(_speakers)
                {
                    var sl = _speakers.Dequeue();
                    lock (sl)
                    {
                        if (IsSpeeking)
                        {
                            Wait(100);
                        }
                        else
                        {
                            Thread = new Thread(new ThreadStart(() => { Speaking(sl); }));
                            Thread.Start();

                        }
                    }
                }
                   
            }
        }

        private void Wait(int v)
        {
            Thread .Sleep(v);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saying"></param>
        private void Speaking(string saying)
        {
            IsSpeeking = true;
            string say = saying;
            Thread task = new Thread(new ThreadStart(() =>
            {
                SpeechSynthesizer speech = new SpeechSynthesizer();
                speech.Volume = 100; //音量
                CultureInfo keyboardCulture = System.Windows.Forms.InputLanguage.CurrentInputLanguage.Culture;
                InstalledVoice neededVoice = speech.GetInstalledVoices(keyboardCulture).FirstOrDefault();
                if (neededVoice == null)
                {
                    say = "未知的操作";
                }
                else
                {
                    speech.SelectVoice(neededVoice.VoiceInfo.Name);
                }
                speech.Speak(say);
                IsSpeeking = false;
            })); 
            task.Start();
        }
    }
}
