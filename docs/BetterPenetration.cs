using System.IO;
using BepInEx;
using UnityEngine;

namespace HS2Wiki.docs
{
    public class BetterPenetration
    {
        private static Texture2D _image_1, _image_2, _image_3, _image_4, _image_5, _image_6, _image_7, _image_8, _image_9, _image_10, _image_11, _image_12, _image_13;

        public static void Init() {
            _image_1  = "BP_1.png".LoadTexture();
            _image_2  = "BP_2.png".LoadTexture();
            _image_3  = "BP_3.png".LoadTexture();
            _image_4  = "BP_4.png".LoadTexture();
            _image_5  = "BP_5.png".LoadTexture();
            _image_6  = "BP_6.png".LoadTexture();
            _image_7  = "BP_7.png".LoadTexture();
            _image_8  = "BP_8.png".LoadTexture();
            _image_9  = "BP_9.png".LoadTexture();
            _image_10 = "BP_10.png".LoadTexture();
            _image_11 = "BP_11.png".LoadTexture();
            _image_12 = "BP_12.png".LoadTexture();
            _image_13 = "BP_13.png".LoadTexture();

            WikiPlugin.PublicAPI?.RegisterPage("BetterPenetration", "Instructions", Instructions);
            WikiPlugin.PublicAPI?.RegisterPage("BetterPenetration", "FAQ", FAQ);
        }   

        public static void Instructions() {
            GUILayout.Label("<size=30>Better Penetration Studio Instructions.</size>");
            GUILayout.TextArea("First and foremost, make sure that you have all of the necessary plugins installed." +
                "A vanilla install of the game requires a long list of plugins to be installed first, " + 
                "too many for me to list. It is best to start with a repack, " +
                "someone else has done all of that work for you already so you should take advantage of it.\n\n" + 
                "Make sure you are using the latest version of BetterPenetration, this can be found on my Patreon page <link=https://www.patreon.com/Animal42069>https://www.patreon.com/Animal42069</link>\n\n\n" +
                "Common plugins that you may be missing but are definitely required:\n\n" +
                "BonesFramework: this Plugin requires some additions to the latest version of BonesFramework that is out there. I have a test build available on my patreon page that should have been included with this package. I have contacted the author of BonesFramework and requested that these additions be rolled in to the official released version. Until that happens, this is the only version that will work.\n\n" +
                "UncensorSelector 3.10 or later: <link=https://www.patreon.com/DeathWeasel/posts?filters[tag]=UncensorSelector>https://www.patreon.com/DeathWeasel/posts?filters[tag]=UncensorSelector</link>");

            GUILayout.Space(10);

            GUILayout.Label("<size=20>What this Plugin Does</size>");
            GUILayout.TextArea("This plugin is intended to be used with BP Penis and Vagina uncensors. " +
                "The plugin adds in all of the necessary dynamic bones and colliders to the uncensors and automatically " +
                "adjusts them for better looking penetration. The plugin also dynamically adjusts the location of the penis " +
                "and will automatically adjust the length and girth of the penis during penetration. Controls are included to " +
                "adjust collider sizes and to change the penis squish behavior to your tastes. Setup is designed to be as user " +
                "friendly as possible, and requires adding as little as one or two node constraints.");

            GUILayout.Space(10);

            GUILayout.Label("<size=20>How it Works</size>");
            GUILayout.TextArea("The BP penis uncensors add two control bones, \"k_f_dan_entry\" and \"k_f_dan_end\". " +
                "The penis will first track the entry position, then after it has passed the entry position it will track the end position. " +
                "The penis will begin to squish after passing the entry point. The end point is a hard limit, so the penis will never pass this " +
                "point and squish down to avoid doing so.");

            GUILayout.Space(10);

            GUILayout.Label("<size=20>Instructions</size>");
            GUILayout.TextArea("First make sure that you have are requirement correctly installed (see above).\n" +
                "Next make sure that you have a BP penis uncensor selected for the male (or futa) and a BP vagina uncensor selected for the female.");

            if (_image_1 != null)
                GUILayout.Box(_image_1, GUILayout.Width(477), GUILayout.Height(274));

            GUILayout.Space(10);

            GUILayout.Label("<size=20>Studio Controls (requires Studio_BetterPenetrationControls plugin)</size>");
            GUILayout.TextArea("Squish Threshold controls how far the penis passes the entry point before it starts to squish. " +
                "Far Left means it will immediately begin to squish, far Right means it will never squish.\n\n" +
                "Length Squish controls how much the penis length decreases when squishing. Far Left means no decrease in length.\n\n" +
                "Girth Squish controls how much the penis girth increases when squishing. Far Left means no increase in girth.\n\n" +
                "Collider controls are pretty self-explanatory. They adjust the size and position of the colliders that make up the penis.\n\n" +
                "These settings will be saved into the scene and loaded when the scene loads. Without the controls plugin, you can still load in settings saved to a scene but cannot make adjustments.");

            GUILayout.Space(10);

            GUILayout.Label("<size=20>Detailed Instructions</size>");
            GUILayout.Label("<size=15><b>Step 1</b></size>");
            GUILayout.TextArea("Use NodeConstraints and parent the male \"k_f_dan_entry\" bone to the female \"cf_J_Vagina_root\" bone.\n" +
    	        "    a) Open up NodeConstraints (ctrl + N)\n" +
    	        "    b) Select female character in workspace\n" +
    	        "    c) Check Advanced List\n" +
    	        "    d) Search for vagina\n" +
    	        "    e) Select \"cf_J_Vagina_root\"\n" +
    	        "    f) Select Set as parent");

            GUILayout.Space(10);

            GUILayout.Label("<size=15><b>Step 2</b></size>");
            GUILayout.TextArea("Use NodeConstraints and parent the male \"k_f_dan_entry\" bone to the female \"cf_J_Vagina_root\" bone.\n" +
    	        "    a) Open up NodeConstraints (ctrl + N)\n" +
    	        "    b) Select female character in workspace\n" +
    	        "    c) Check Advanced List\n" +
    	        "    d) Search for vagina\n" +
    	        "    e) Select \"cf_J_Vagina_root\"\n" +
    	        "    f) Select Set as parent");

            if (_image_2 != null)
                GUILayout.Box(_image_2, GUILayout.Width(716), GUILayout.Height(567));

            GUILayout.TextArea(
    	        "    g) Select male character in workspace\n" +
    	        "    h) Search for entry\n" +
    	        "    i) Select \"k_f_dan_entry\"\n" +
    	        "    j) Select Set as child\n" +
    	        "    k) Select Link position\n" +
    	        "    l) Select Add new");

            if (_image_3 != null)
                GUILayout.Box(_image_3, GUILayout.Width(714), GUILayout.Height(569));

            GUILayout.Space(10);

            GUILayout.Label("<size=15><b>Step 2</b></size>");
            GUILayout.TextArea("At this point all colliders and bones should be moving correctly, " +
                "but the penis can be further adjusted to improve penetration in some cases. " +
                "This can be accomplished by parenting the \"k_f_dan_end\" bone to an internal bone such as \"cf_J_Spine02\".\n\n" +
                "Follow the same instructions above but substitute \"cf_J_Spine02\" for \"cf_J_Vagina_root\" and \"k_f_dan_end\" for \"k_f_dan_start\"");

            if (_image_4 != null)
                GUILayout.Box(_image_4, GUILayout.Width(713), GUILayout.Height(610));

            GUILayout.Label("<size=15>Without \"k_f_dan_end\" parented</size>");
            if (_image_5 != null)
                GUILayout.Box(_image_5, GUILayout.Width(712), GUILayout.Height(321));

            GUILayout.Label("<size=15>With \"k_f_dan_end\" parented</size>");
            if (_image_6 != null)
                GUILayout.Box(_image_6, GUILayout.Width(711), GUILayout.Height(416));
        }
    
        public static void FAQ() {
            GUILayout.Label("<size=30>Better Penetration Frequently Asked Questions</size>");
            GUILayout.Label("<size=20>It's not working! The girl's vagina is sinking to the floor!</size>");
            GUILayout.TextArea("First and foremost, make sure that you have all of the necessary plugins installed. " +
                "A vanilla install of the game requires a long list of plugins to be installed first, too many for me to list. " +
                "It is best to start with a repack, someone else has done all of that work for you already so you should take advantage of it.\n\n" +
                "Make sure you are using the latest version of BetterPenetration, this can be found on my Patreon page <link=https://www.patreon.com/Animal42069>https://www.patreon.com/Animal42069</link>\n\n\n" +
                "Common plugins that you may be missing but are definitely required: \n\n" +
                "BonesFramework 1.4.1 or later: <link=https://joan6694.bitbucket.io/>https://joan6694.bitbucket.io/</link>\n\n" +
                "UncensorSelector 3.10 or later: <link=https://www.patreon.com/DeathWeasel/posts?filters[tag]=UncensorSelector>https://www.patreon.com/DeathWeasel/posts?filters[tag]=UncensorSelector</link>");
        
            GUILayout.Space(10);

            GUILayout.Label("<size=20>The girl's vagina isn't animating</size>");
            GUILayout.TextArea("Animation is only automatically set up in game, studio requires manual setup.\n\n" +
                "    1) If you are in game and it still isn't animating, make sure you have one of the BP uncensors selected for your girl. These uncensors all start with bp.\n" +
                "    2) If your uncensor is Default, then a different might get selected as your uncensor. Open Plugin settings and exclude the other uncensors\n" +
                "    3) Alternatively set one of the BP uncensors as your default");

            if (_image_7 != null)
                GUILayout.Box(_image_7, GUILayout.Width(697), GUILayout.Height(235));

            GUILayout.Space(10);

            GUILayout.Label("<size=20>My penis is clipping through the girl!</size>");
            GUILayout.TextArea("SAC Sausage is not compatible with BetterPenetration.\n\n" +
                "    1) Make sure it is not selected as your penis uncensor.\n" +
                "    2) If your uncensor is Default, then SAC Sausage might get selected as your uncensor. Open Plugin settings and exclude SAC Sausage\n" +
                "    3) Alternatively set one of the other penis uncensors as your default");

            if (_image_8 != null)
                GUILayout.Box(_image_8, GUILayout.Width(697), GUILayout.Height(260));

            GUILayout.Space(10);

            GUILayout.Label("<size=20>The girl’s vagina looks sunken and weird in some positions.</size>");
            GUILayout.TextArea("This is not an issue with BetterPenetration or the uncensors, but rather an animation bug. " +
                "The best way to fix it is to use the Advanced Bone Sliders to apply a small Offset Y to the cf_J_Kokan bone. " +
                "You may need to save/reload the character or restart the character editor to get the change to take place. " +
                "There is an option in the BetterPenetration plugin settings to try to guess where the correct position should be, " +
                "but its better to make the adjustment to the character.\n\n" +
                "I make it a habit to make this adjustment on every girl I use.");

            if (_image_9 != null)
                GUILayout.Box(_image_9, GUILayout.Width(617), GUILayout.Height(372));

            GUILayout.Space(10);

            GUILayout.Label("<size=20>How do I open the Advanced Bone Slider to make these adjustments?</size>");
            GUILayout.TextArea("In the Character Editor, bottom right, click the Advanced Bone Slider button");

            if (_image_10 != null)
                GUILayout.Box(_image_10, GUILayout.Width(436), GUILayout.Height(242));

            GUILayout.Space(10);

            GUILayout.Label("<size=20>The girl’s vagina but clips through her underwear.</size>");
            GUILayout.TextArea("This can happen with bigger butts and using the BetterPenetration uncensors. " +
                "This happens because I had to steal some of the mesh weights away from these areas to create more convincing looking penetration. " +
                "These clipping issues can usually be solved by making adjustments to the character. Search for the cf_J_Siri_s_L / cf_J_Siri_s_R bones. " +
                "An Offset to the X coordinate will usually fix the clipping.");

            if (_image_11 != null)
                GUILayout.Box(_image_11, GUILayout.Width(713), GUILayout.Height(227));
            if (_image_12 != null)
                GUILayout.Box(_image_12, GUILayout.Width(714), GUILayout.Height(223));

            GUILayout.Space(10);

            GUILayout.Label("<size=20>How do I adjust penis size?</size>");
            GUILayout.TextArea("Penis size can no longer be controlled through the BetterPenetration plugin. " +
                "Controlling it through a plugin was horribly inefficient and cause problems with a lot of the HScene animations. " +
                "Instead you should use the character editor and the Advanced Bone Sliders window to adjust penis size.\n\n" +
                "    1) Search for dan in the Advanced Bone Sliders window.\n" +
                "    2) Select <b>cm_J_dan101_00</b> to adjust penis.\n" +
                "    3) Use Scale X/Y/Z to adjust penis. Z adjusts length, X and Y adjust girth.\n" +
                "    4) Select <b>cm_J_dan109_00</b> to adjust penis head\n" +
                "    5) Select <b>cm_J_dan_f_top</b> to adjust the sack.\n" +
                "    6) Use Scale X/Y/Z to adjust sack size.");

            if (_image_13 != null)
                GUILayout.Box(_image_13, GUILayout.Width(587), GUILayout.Height(708));
        }
    }

}
