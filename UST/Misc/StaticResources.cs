namespace USTManager.Misc
{
    public static class StaticResources
    {
        public static readonly string Readme = """
        # An UST is made up of 3 parts:
        
        - soundtrack.ust : It can be named anything BUT template.ust, otherwise your UST won't show up in game. This is the main file that contains all the information about the UST. Please use the template.ust file as a base.
        
        - Any .mp3, .ogg and .wav files : These are the audio files that the UST uses. They can be named anything.

        - icon.png : This is the icon that will be displayed in the selection screen. It HAS to be .png.

        # Some extra notes:

        USTManager can replace any audio in the game if you know the name of the clip.
        Simply add a "global" entry to your .ust file and set "Part" to the clip name and "Path" to the path of the replacement.
        
        There are 2 commands in game. One just enables or disables USTManager (ust.toggle) and the other makes getting clip names a bit easier (ust.debug).
        """;
    }
}