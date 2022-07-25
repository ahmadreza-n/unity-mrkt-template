using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Tts
{
  /**
   * Implementation of ITextToSpeech using the windows speech synthesis for holo lens.
   * An abstraction layer for TextToSpeechImp
   */
  public class UWPTextToSpeech : ITextToSpeech
  {
    private readonly TextToSpeechImp textToSpeechImp;

    public UWPTextToSpeech(TextToSpeechImp textToSpeechImp)
    {
      this.textToSpeechImp = textToSpeechImp;
    }

    /**
     * Speaks SSML and returns a synthesis object used for tracks and timings.
     */
    public ITextToSpeechSynthesis SpeakSsml(string ssml)
    {
#if WINDOWS_UWP
            Tuple<AudioSource, AudioClip, List<TtsTrack>> tupleRes = textToSpeechImp.SpeakSsml(ssml);
            if (tupleRes != null)
            {
                AudioSource source = tupleRes.Item1;
                AudioClip clip = tupleRes.Item2;
                List<TtsTrack> tracks = tupleRes.Item3;
                source.clip = clip;
                // The delay between these two lines needs to be close to 0 to ensure cues are placed at the correct times.
                // From testing it seems like this is okay.
                source.Play();
                return new UWPTextToSpeechSynthesis(tracks, source);
            }
            throw new Exception("Something went wrong with UWP SSML TTS");
#else
      Debug.LogWarningFormat("Text to Speech not supported in editor.\n\"{0}\"", ssml);
      return null;
#endif
    }
  }


  /**
   * Object to interact with the speech synthesis
   */
  public class UWPTextToSpeechSynthesis : ITextToSpeechSynthesis
  {
    private readonly Dictionary<TrackType, Queue<TtsCue>> tracksQueue;
    private readonly AudioSource source;
    public UWPTextToSpeechSynthesis(List<TtsTrack> tracks, AudioSource source)
    {
      this.source = source;
      tracksQueue = new Dictionary<TrackType, Queue<TtsCue>>();
      Dictionary<TrackType, List<TtsCue>> tracksOrg = new Dictionary<TrackType, List<TtsCue>>();

      // Combine tracks by TrackType
      foreach (var ttsTrack in tracks)
      {
        if (tracksOrg.ContainsKey(ttsTrack.TrackType))
        {
          tracksOrg[ttsTrack.TrackType].AddRange(ttsTrack.TtsCues);
        }
        else
        {
          tracksOrg[ttsTrack.TrackType] = ttsTrack.TtsCues;
        }
      }

      // Sort Cues and add to Queue
      foreach (var kv in tracksOrg)
      {
        tracksQueue[kv.Key] = new Queue<TtsCue>(kv.Value.OrderBy((c) => c.StartTime));
      }
    }

    public bool HasRemainingCues(TrackType type)
    {
      return tracksQueue[type].Count != 0;
    }

    /**
     * Uses a queue to return a list of cues that have not been processed but have happened before the current playback time.
     * It is the responsibility of any implementations to ensure that relativeTime is accurate.
     *
     * This could be improved but works well now, especially with use of update() functions
     */
    public List<TtsCue> GetTrackCuesForRelativeTime(TrackType type, float relativeTime)
    {
      List<TtsCue> happenedTrackCues = new List<TtsCue>();
      if (tracksQueue.Count == 0)
      {
        return happenedTrackCues;
      }

      while (tracksQueue[type].Count != 0 && tracksQueue[type].Peek().StartTime.TotalSeconds < relativeTime)
      {
        happenedTrackCues.Add(tracksQueue[type].Dequeue());
      }

      return happenedTrackCues;
    }

    public bool IsDone()
    {
      return !source.isPlaying;
    }
  }
}