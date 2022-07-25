using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Tts;

public class TTPHelper : MonoBehaviour
{
  [SerializeField]
  [Tooltip("Message to Speak and Show")]
  [TextArea(10, 20)]
  private string message;
  public string Message
  {
    get => message;
    set => message = value;
  }

  [SerializeField]
  [Tooltip("TMP Game Object")]
  private GameObject tmpGameObject;
  public GameObject TMPGameObject
  {
    get => tmpGameObject;
    set => tmpGameObject = value;
  }

  public UnityEvent finishEvent = new UnityEvent();
  public UnityEvent instantFinishEvent = new UnityEvent();

  private TextToSpeechImp textToSpeech;
  private TMP_Text tmp;
  private ITextToSpeech ttp;
  private ITextToSpeechSynthesis ttsSynthesis;
  private float timer = 0.0f;
  private Queue<string> linesQ, wordsQ;
  private bool isDone = false;

  private static string ssmlMainTag = @"<speak version='1.0' " +
                                      "xmlns='http://www.w3.org/2001/10/synthesis' " +
                                      "xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' " +
                                      "xsi:schemaLocation='http://www.w3.org/2001/10/synthesis  " +
                                      "http://www.w3.org/TR/speech-synthesis/synthesis.xsd' " +
                                      "xml:lang='en-US'>" +
                                      "{0}" +
                                      "</speak>";
  private static float static_cc_delay = 0.2f;

  private void TTP_CC() // returns true when done.
  {
    if (ttsSynthesis == null || (ttsSynthesis.IsDone() && !ttsSynthesis.HasRemainingCues(TrackType.SpeechWord)))
    {
      if (linesQ.Count != 0)
      {
        string line = linesQ.Dequeue();
        if (!message.StartsWith(line))
          tmp.text += '\n';
        wordsQ = new Queue<string>(line.Split(' '));
        string msgSSML = string.Format(ssmlMainTag, line);

        Debug.LogFormat("Speaking SSML: {0}", msgSSML);

        ttsSynthesis = ttp.SpeakSsml(msgSSML);
      }
      else if (!isDone)
      {
        isDone = true;
        instantFinishEvent.Invoke();
      }
      timer = 0;
    }
    else
    {
      List<TtsCue> cues = ttsSynthesis.GetTrackCuesForRelativeTime(TrackType.SpeechWord, timer);
      foreach (var ttsCue in cues)
        tmp.text += wordsQ.Dequeue() + " ";
    }
  }

  void CC()
  {
    if (timer > static_cc_delay)
    {
      if (wordsQ.Count != 0)
      {
        timer = 0;
        tmp.text += wordsQ.Dequeue() + " ";
      }
      else if (linesQ.Count != 0)
      {
        timer = 0;
        string line = linesQ.Dequeue();
        if (!message.StartsWith(line))
          tmp.text += '\n';
        wordsQ = new Queue<string>(line.Split(' '));
      }
      else if (!isDone)
      {
        isDone = true;
        instantFinishEvent.Invoke();
      }
    }
  }

  // Start is called before the first frame update
  void Start()
  {
    textToSpeech = tmpGameObject.GetComponent<TextToSpeechImp>();
    if (textToSpeech == null)
    {
      textToSpeech = tmpGameObject.AddComponent<TextToSpeechImp>();
      textToSpeech.Voice = TextToSpeechVoice.Zira;
    }
    tmp = tmpGameObject.GetComponent<TMP_Text>();
    tmp.text = "";
    Debug.LogFormat("Raw ttp message: {0}", message);
    ttp = new UWPTextToSpeech(textToSpeech);
    linesQ = new Queue<string>(message.Split('\n'));
    wordsQ = new Queue<string>();

#if !WINDOWS_UWP
    Debug.LogWarningFormat("Text to Speech not supported in editor.\n\"{0}\"", message);
#endif
  }

  // Update is called once per frame
  void Update()
  {
    timer += Time.deltaTime;
#if WINDOWS_UWP
    TTP_CC();
#else
    CC();
#endif
  }
}
