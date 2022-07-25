using System;
using System.Collections.Generic;

namespace Tts
{
  /**
   * Type of cue from SSML
   */
  public enum TrackType
  {
    SpeechWord,
    SpeechBookmark
  }

  public interface ITextToSpeech
  {
    /**
     * Uses implemented engine to speak SSML.
     * Returns a ITextToSpeechSynthesis to handle callbacks
     */
    ITextToSpeechSynthesis SpeakSsml(string ssml);
  }

  public interface ITextToSpeechSynthesis
  {
    /**
     * Returns true if there are still unspoken cues
     */
    bool HasRemainingCues(TrackType type);

    /**
     * Returns list of cues that have been spoken but not previously processed. 
     */
    List<TtsCue> GetTrackCuesForRelativeTime(TrackType type, float relativeTime);

    /**
     * Audio source is done
     */
    bool IsDone();
  }

  /**
   * Track object representing a track of cues from SSML
   */
  public class TtsTrack
  {
    public TrackType TrackType { get; set; }
    public List<TtsCue> TtsCues { get; set; }

    public TtsTrack(TrackType trackType, List<TtsCue> ttsCues)
    {
      TrackType = trackType;
      TtsCues = ttsCues;
    }
  }

  /**
   * Cue describing a time based event in a SSML track
   * EG. Word boundary or SSML mark
   */
  public class TtsCue
  {
    public TimeSpan Duration { get; set; }
    public int? EndPositionInInput { get; set; }
    public string Id { get; set; }
    public int? StartPositionInInput { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Text { get; set; }

    public TtsCue(TimeSpan duration, int? endPositionInInput, string id, int? startPositionInInput, TimeSpan startTime, string text)
    {
      Duration = duration;
      EndPositionInInput = endPositionInInput;
      Id = id;
      StartPositionInInput = startPositionInInput;
      StartTime = startTime;
      Text = text;
    }
  }
}