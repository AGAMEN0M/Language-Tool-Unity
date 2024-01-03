using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "NewTMP_LanguageFontList", menuName = "Language/Language Font List Data (TMP)")]
public class TMP_LanguageFontListData : ScriptableObject
{
    public List<TMP_FontAsset> TMP_fontList; //List of fonts for the LanguageText script.
}
