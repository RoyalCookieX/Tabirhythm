using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Tabirhythm.Editor
{
    [CustomTimelineEditor(typeof(NoteClip))]
    public class NoteClipTimelineEditor : ClipEditor
    {
        private static Texture2D s_keyframeTexture;
        private static Color s_clipColor = new Color(0.4f, 0.4f, 0.4f);
        private static Color s_windowColor = Color.white;

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            EditorGUI.DrawRect(region.position, s_clipColor);

            NoteClip noteClip = (NoteClip)clip.asset;
            if (!noteClip)
                return;

            float pixelsPerSecond = region.position.width / (float)(region.endTime - region.startTime);
            float clipHeight = region.position.height;
            double startTime = noteClip.Window.time;
            double endTime = startTime + noteClip.Window.holdDuration;
            switch (noteClip.Window.action)
            {
                case NoteAction.Rest: break;
                case NoteAction.Step:
                    {
                        DrawKeyframe(startTime, pixelsPerSecond, clipHeight, s_windowColor);
                        break;
                    }
                case NoteAction.Hold:
                    {
                        Rect windowRect = new Rect(
                            (float)startTime * pixelsPerSecond,
                            0.0f,
                            (float)noteClip.Window.holdDuration * pixelsPerSecond,
                            clipHeight
                        );
                        EditorGUI.DrawRect(windowRect, s_windowColor);
                        DrawKeyframe(startTime, pixelsPerSecond, clipHeight, s_windowColor);
                        DrawKeyframe(endTime, pixelsPerSecond, clipHeight, s_windowColor);
                        break;
                    }
            }
        }

        public override void OnClipChanged(TimelineClip clip)
        {
            NoteClip noteClip = (NoteClip)clip.asset;
            if (!noteClip)
                return;
            clip.displayName = noteClip.Prefab ? noteClip.Prefab.name : "None";
        }

        private void DrawKeyframe(double time, float pixelsPerSecond, float clipHeight, Color color)
        {
            if (!s_keyframeTexture)
                s_keyframeTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Tabirhythm/Editor/Sprites/Keyframe.png");

            float size = clipHeight * 0.5f;
            Rect keyframeRect = new Rect(
                (float)time * pixelsPerSecond - size * 0.5f,
                (clipHeight - size) * 0.5f,
                size,
                size
            );
            GUI.DrawTexture(keyframeRect, s_keyframeTexture, ScaleMode.StretchToFill, true, 1.0f, color, 0.0f, 0.0f);
        }
    }
}
