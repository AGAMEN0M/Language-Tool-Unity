using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLanguageFontList", menuName = "Language/Language Font List Data")]
public class LanguageFontListData : ScriptableObject
{
    public List<Font> fontList; // List of fonts for the LanguageText script.
}