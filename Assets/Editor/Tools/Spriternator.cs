using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;

public class SpriterNator : EditorWindow
{
    private Texture2D spriteSheet;
    private string pathToSave;
    private string spriteLibraryName;
    private int frameRate = 12;
    private List<AnimationDetails> animationsList = new List<AnimationDetails>();
    private List<Sprite> allSprites;
    private Vector2 scrollPosition;
    private Texture2D myLogo;

    // Animation 정보 구조체 (X/Y 범위 기반)
    [System.Serializable]
    private class AnimationDetails
    {
        public string name;
        public int startX;
        public int endX;
        public int startY;
        public int endY;
        public bool shouldLoop;
        public int frameRate;
    }

    [MenuItem("Tools/SpriterNator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SpriterNator));
    }

    private void OnEnable()
    {
        myLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Tools/lg.png");
        if (animationsList.Count == 0)
        {
            animationsList.Add(new AnimationDetails());
        }
    }

    private void OnGUI()
    {
        // ------------------------------ Styling ---------------------------------------------------
        GUIStyle customHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 50,
            alignment = TextAnchor.MiddleCenter,
            margin = new RectOffset(0, 0, 0, 30),
            fontStyle = FontStyle.Bold
        };

        GUIStyle customButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 18,
            margin = new RectOffset(5, 5, 10, 10),
            fontStyle = FontStyle.Bold
        };

        GUIStyle saveloadButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 13,
            margin = new RectOffset(5, 5, 10, 10),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(10, 10, 5, 5)
        };

        GUIStyle customSectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUIStyle textstyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
        GUIStyle textfieldstyle = new GUIStyle(GUI.skin.textField) { fontSize = 12 };
        GUIStyle intFieldStyle = new GUIStyle(EditorStyles.numberField)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };

        // ------------------------------ UI ----------------------------------------------------------
        GUILayout.BeginArea(new Rect(10, 20, position.width - 20, position.height - 20));
        GUILayout.Label("SPRITERNATOR", customHeaderStyle);

        // Save / Load Animations List
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SAVE LIST", saveloadButtonStyle, GUILayout.Width(100)))
        {
            string path = EditorUtility.SaveFilePanel("Save Animations List for Future Use.", "", "AnimationsList", "json");
            if (!string.IsNullOrEmpty(path))
            {
                SaveAnimationsList(path);
            }
        }
        GUILayout.Space(10);
        if (GUILayout.Button("LOAD LIST", saveloadButtonStyle, GUILayout.Width(100)))
        {
            string path = EditorUtility.OpenFilePanel("Load Previously Created Animations List.", "", "json");
            if (!string.IsNullOrEmpty(path))
            {
                LoadAnimationsList(path);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        // Properties
        GUILayout.Space(20);
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
        GUILayout.BeginVertical("box");
        GUILayout.Label("PROPERTIES", customSectionStyle);
        GUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        GUILayout.Space(20);

        GUILayout.BeginVertical("box");

        // Save Path
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Save Path:", textstyle, GUILayout.Width(100));
        pathToSave = EditorGUILayout.TextField("", pathToSave, textfieldstyle, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("...", GUILayout.Width(50)))
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder To Save Animations In. ", "", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                if (folderPath.StartsWith(Application.dataPath))
                    folderPath = "Assets" + folderPath.Substring(Application.dataPath.Length);

                pathToSave = folderPath;
                GUI.FocusControl(null);
            }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Sprite Sheet
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sprite Sheet:", textstyle, GUILayout.Width(100));
        spriteSheet = (Texture2D)EditorGUILayout.ObjectField(spriteSheet, typeof(Texture2D), false, GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

        // Sprite Library Name
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Library Name:", textstyle, GUILayout.Width(100));
        spriteLibraryName = EditorGUILayout.TextField("", spriteLibraryName, textfieldstyle);
        EditorGUILayout.EndHorizontal();
        GUILayout.EndVertical();

        // ------------------------------ Animations List ---------------------------------------------------
        GUILayout.Space(20);
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.8f, 1.0f);
        GUILayout.BeginVertical("box");
        GUILayout.Label("ANIMATIONS LIST", customSectionStyle);
        GUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);

        if (GUILayout.Button(new GUIContent("+", "Add Animation"), customButtonStyle))
            animationsList.Add(new AnimationDetails());
        GUILayout.Space(10);

        GUILayout.BeginVertical("box");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = 0; i < animationsList.Count; i++)
        {
            GUILayout.BeginHorizontal();

            // Move Up / Down
            if (GUILayout.Button("▲", GUILayout.Width(25)) && i > 0)
            {
                var temp = animationsList[i];
                animationsList[i] = animationsList[i - 1];
                animationsList[i - 1] = temp;
            }
            if (GUILayout.Button("▼", GUILayout.Width(25)) && i < animationsList.Count - 1)
            {
                var temp = animationsList[i];
                animationsList[i] = animationsList[i + 1];
                animationsList[i + 1] = temp;
            }
            GUILayout.Space(10);
            // Animation Name
            GUILayout.Label("Name:", GUILayout.Width(50));
            animationsList[i].name = EditorGUILayout.TextField(animationsList[i].name, textfieldstyle, GUILayout.Width(150));

            // X/Y 범위 입력
            GUILayout.Label("SX:", GUILayout.Width(50));
            animationsList[i].startX = EditorGUILayout.IntField(animationsList[i].startX, intFieldStyle, GUILayout.Width(50));

            GUILayout.Label("EX:", GUILayout.Width(50));
            animationsList[i].endX = EditorGUILayout.IntField(animationsList[i].endX, intFieldStyle, GUILayout.Width(50));

            GUILayout.Label("SY:", GUILayout.Width(50));
            animationsList[i].startY = EditorGUILayout.IntField(animationsList[i].startY, intFieldStyle, GUILayout.Width(50));

            GUILayout.Label("EY:", GUILayout.Width(50));
            animationsList[i].endY = EditorGUILayout.IntField(animationsList[i].endY, intFieldStyle, GUILayout.Width(50));

            // Frame Rate
            animationsList[i].frameRate = EditorGUILayout.IntField("Frame Rate:", animationsList[i].frameRate, intFieldStyle);

            // Loop
            animationsList[i].shouldLoop = EditorGUILayout.Toggle("Loop Enabled:", animationsList[i].shouldLoop);

            // Remove / Duplicate
            if (GUILayout.Button("-", GUILayout.Width(30)))
                animationsList.RemoveAt(i);
            if (GUILayout.Button("++", GUILayout.Width(50)))
            {
                var copy = new AnimationDetails
                {
                    name = animationsList[i].name + " Copy",
                    startX = animationsList[i].startX,
                    endX = animationsList[i].endX,
                    startY = animationsList[i].startY,
                    endY = animationsList[i].endY,
                    shouldLoop = animationsList[i].shouldLoop,
                    frameRate = animationsList[i].frameRate
                };
                animationsList.Insert(i + 1, copy);
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // -------------------------- Buttons ----------------------------------------------------------------
        GUILayout.Space(20);
        if (GUILayout.Button("CLEAR ANIMATIONS", customButtonStyle))
            animationsList.Clear();

        if (GUILayout.Button("CREATE ANIMATIONS", customButtonStyle))
        {
            LoadAllSprites();
            foreach (var anim in animationsList)
                CreateAnimationFromFrames(anim);
        }

        if (GUILayout.Button("CREATE SPRITE LIBRARY", customButtonStyle))
        {
            LoadAllSprites();
            CreateSpriteLibrary();
        }

        GUILayout.EndArea();
    }

    // ------------------------------ Load Sprites -----------------------------------------------
    private void LoadAllSprites()
    {
        if (spriteSheet == null)
        {
            Debug.LogError("SpriterNator: SpriteSheet not assigned.");
            return;
        }

        Resources.UnloadUnusedAssets();
        string path = AssetDatabase.GetAssetPath(spriteSheet);
        allSprites = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .ToList();

        // y_x 기반 정렬
        allSprites.Sort((s1, s2) =>
        {
            string[] parts1 = s1.name.Split('_');
            string[] parts2 = s2.name.Split('_');

            int y1 = int.TryParse(parts1[0], out int ty1) ? ty1 : 0;
            int x1 = int.TryParse(parts1[1], out int tx1) ? tx1 : 0;
            int y2 = int.TryParse(parts2[0], out int ty2) ? ty2 : 0;
            int x2 = int.TryParse(parts2[1], out int tx2) ? tx2 : 0;

            int cmp = y1.CompareTo(y2);
            return cmp != 0 ? cmp : x1.CompareTo(x2);
        });
    }

    // ------------------------------ Create Animation Clip --------------------------------------
    private int GetY(Sprite s)
    {
        string[] parts = s.name.Split('_');
        return int.TryParse(parts.Length >= 2 ? parts[^2] : "0", out int y) ? y : 0;
    }

    private int GetX(Sprite s)
    {
        string[] parts = s.name.Split('_');
        return int.TryParse(parts.Length >= 1 ? parts[^1] : "0", out int x) ? x : 0;
    }

    private AnimationClip CreateAnimationFromFrames(AnimationDetails animDetail)
    {
        if (allSprites == null || allSprites.Count == 0)
        {
            Debug.LogError("No sprites loaded.");
            return null;
        }

        // X/Y 범위 기반으로 스프라이트 선택
        //List<Sprite> selectedSprites = allSprites
        //    .Where(s =>
        //    {
        //        string[] parts = s.name.Split('_');
        //        int y = int.TryParse(parts[0], out int ty) ? ty : 0;
        //        int x = int.TryParse(parts[1], out int tx) ? tx : 0;
        //        return y >= animDetail.startY && y <= animDetail.endY && x >= animDetail.startX && x <= animDetail.endX;
        //    })
        //    .OrderBy(s => int.Parse(s.name.Split('_')[0]))
        //    .ThenBy(s => int.Parse(s.name.Split('_')[1]))
        //    .ToList();
        List<Sprite> selectedSprites = allSprites
        .Where(s =>
        {
            string[] parts = s.name.Split('_');
            // 뒤에서 두 번째 값이 Y, 마지막 값이 X
            int y = int.TryParse(parts[^2], out int ty) ? ty : 0;
            int x = int.TryParse(parts[^1], out int tx) ? tx : 0;
            return y >= animDetail.startY && y <= animDetail.endY && x >= animDetail.startX && x <= animDetail.endX;
        })
        .OrderBy(s => int.TryParse(s.name.Split('_')[^2], out int y) ? y : 0) // Y 기준 정렬
        .ThenBy(s => int.TryParse(s.name.Split('_')[^1], out int x) ? x : 0)   // X 기준 정렬
        .ToList();

        AnimationClip clip = new AnimationClip { frameRate = animDetail.frameRate };
        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        List<ObjectReferenceKeyframe> keyframes = new List<ObjectReferenceKeyframe>();
        for (int i = 0; i < selectedSprites.Count; i++)
        {
            keyframes.Add(new ObjectReferenceKeyframe
            {
                time = i / (float)animDetail.frameRate,
                value = selectedSprites[i]
            });
        }

        if (animDetail.shouldLoop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes.ToArray());

        string fullPath = System.IO.Path.Combine(pathToSave, animDetail.name + ".anim");
        AssetDatabase.CreateAsset(clip, fullPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return AssetDatabase.LoadAssetAtPath<AnimationClip>(fullPath);
    }

    // ------------------------------ Create Sprite Library --------------------------------------
    private void CreateSpriteLibrary()
    {
        SpriteLibraryAsset spriteLibraryAsset = ScriptableObject.CreateInstance<SpriteLibraryAsset>();

        foreach (var anim in animationsList)
        {
            List<Sprite> sprites = allSprites
                .Where(s =>
                {
                    string[] parts = s.name.Split('_');
                    int y = int.TryParse(parts[0], out int ty) ? ty : 0;
                    int x = int.TryParse(parts[1], out int tx) ? tx : 0;
                    return y >= anim.startY && y <= anim.endY && x >= anim.startX && x <= anim.endX;
                })
                .OrderBy(s => int.Parse(s.name.Split('_')[0]))
                .ThenBy(s => int.Parse(s.name.Split('_')[1]))
                .ToList();

            for (int i = 0; i < sprites.Count; i++)
            {
                spriteLibraryAsset.AddCategoryLabel(sprites[i], anim.name, i.ToString());
            }
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath(pathToSave + "/" + spriteLibraryName + ".asset");
        AssetDatabase.CreateAsset(spriteLibraryAsset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SpriteLibraryAsset createdAsset = AssetDatabase.LoadAssetAtPath<SpriteLibraryAsset>(assetPath);
        if (createdAsset != null)
        {
            EditorGUIUtility.PingObject(createdAsset);
            Selection.activeObject = createdAsset;
        }
    }

    // ------------------------------ Save / Load Animations List ---------------------------------
    [System.Serializable]
    public class SerializableListWrapper<T> { public List<T> List; }

    private void SaveAnimationsList(string filePath)
    {
        string json = JsonUtility.ToJson(new SerializableListWrapper<AnimationDetails> { List = animationsList }, true);
        System.IO.File.WriteAllText(filePath, json);
        AssetDatabase.Refresh();
    }

    private void LoadAnimationsList(string filePath)
    {
        if (!System.IO.File.Exists(filePath)) return;
        string json = System.IO.File.ReadAllText(filePath);
        SerializableListWrapper<AnimationDetails> wrapper = JsonUtility.FromJson<SerializableListWrapper<AnimationDetails>>(json);
        animationsList = wrapper.List;
    }
}
