using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace NounTownZhTW;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BasePlugin
{
    public const string PluginGuid = "ftml.nountown.zhtw";
    public const string PluginName = "NounTown Traditional Chinese (zh-TW)";
    public const string PluginVersion = "0.2.0";

    internal static ManualLogSource Logger = null!;
    internal static string ShadowDir = null!;

    // Persists which Chinese variant ("zh-CN" or "zh-TW") the user last
    // selected for the active/origin language slot. The save file only
    // records the ambiguous editor name "chinese" (shared by both variants),
    // so on the next launch we need this to resolve it correctly - see
    // Patch_ParseLanguageListJson.
    internal static ConfigEntry<string> ActiveChineseVariant = null!;
    internal static ConfigEntry<string> OriginChineseVariant = null!;

    public override void Load()
    {
        Logger = Log;
        ShadowDir = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location)!, "shadow");
        Logger.LogInfo($"zh-TW plugin loading... shadow bundle dir = {ShadowDir}");

        ActiveChineseVariant = Config.Bind("Language", "ActiveChineseVariant", "",
            "Which Chinese variant (zh-CN or zh-TW) the active/learning language should resolve to on next launch. Updated automatically when the language is changed in Settings.");
        OriginChineseVariant = Config.Bind("Language", "OriginChineseVariant", "",
            "Which Chinese variant (zh-CN or zh-TW) the origin/native language should resolve to on next launch. Updated automatically when the language is changed in Settings.");

        ZhTwFont.Load();

        var harmony = new Harmony(PluginGuid);
        harmony.PatchAll();
        Log.LogInfo("zh-TW plugin loaded.");
    }
}

// The "NotoSansCJKsc" TMP_FontAsset's SDF atlas is subsetted to the
// characters actually used by zh-CN content and is missing ~437 glyphs
// needed by zh-TW (Traditional-only characters). Rather than rebuilding the
// static atlas, we switch that font asset to TMP's Dynamic Atlas Population
// Mode and supply a full CJK source font (Noto Sans CJK TC) so missing
// glyphs get rasterized on demand via TMP's FontEngine (FreeType).
//
// TMP_FontAsset.sourceFontFile must be a non-null UnityEngine.Font for the
// dynamic path to even be attempted, but there's no embedded Font asset with
// CJK data anywhere in this game's data files. Rather than constructing one
// via binary asset surgery, we create an empty placeholder Font purely to
// satisfy the null-check, and intercept FontEngine.LoadFontFace(Font, ...)
// (the call TMP's dynamic atlas code makes) to redirect to
// FontEngine.LoadFontFace(byte[]) loaded from our bundled TTF when the
// placeholder is passed in.
internal static class ZhTwFont
{
    internal const string PlaceholderName = "NounTownZhTW_CJK_Fallback";
    internal const string TargetFontAssetName = "NotoSansCJKsc";

    internal static string? FontPath;
    internal static Font? PlaceholderFont;
    private static bool _setupDone;

    internal static void Load()
    {
        var path = Path.Combine(Path.GetDirectoryName(typeof(Plugin).Assembly.Location)!, "fonts", "NotoSansCJKtc-Regular.ttf");
        if (File.Exists(path))
        {
            FontPath = path;
            Plugin.Logger.LogInfo($"[NounTownZhTW] CJK fallback font path: {path} ({new FileInfo(path).Length} bytes)");
        }
        else
        {
            Plugin.Logger.LogError($"[NounTownZhTW] CJK fallback font not found: {path}");
        }
    }

    internal static bool SetupDone => _setupDone;

    internal static void DumpAndSetup(string context)
    {
        try
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            Plugin.Logger.LogInfo($"[FontDiagnostics:{context}] Found {fonts.Length} TMP_FontAsset objects");
            foreach (var f in fonts)
            {
                Plugin.Logger.LogInfo($"[FontDiagnostics:{context}]   \"{f.name}\": atlasPopulationMode={f.atlasPopulationMode} sourceFontFile={(f.sourceFontFile == null ? "null" : f.sourceFontFile.name)} glyphCount={f.glyphTable.Count} charCount={f.characterTable.Count} atlas={f.atlasWidth}x{f.atlasHeight} multiAtlas={f.isMultiAtlasTexturesEnabled}");

                if (!_setupDone && f.name == TargetFontAssetName && FontPath != null)
                {
                    SetupDynamicFallback(f, context);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[FontDiagnostics:{context}] error: {ex}");
        }
    }

    // Lightweight retry called from frequently-firing Harmony patches
    // (GetCountryCode, CheckLoadBundle): unlike DumpAndSetup, this doesn't
    // dump every loaded TMP_FontAsset (which would spam the log on every
    // call while waiting for the game's UI font assets to load) - it just
    // checks whether the target font is loaded yet. Returns true once setup
    // has completed (now or previously).
    internal static bool TryFindAndSetup(string context)
    {
        if (_setupDone || FontPath == null)
        {
            return _setupDone;
        }

        try
        {
            var fonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            foreach (var f in fonts)
            {
                if (f.name == TargetFontAssetName)
                {
                    SetupDynamicFallback(f, context);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"[FontDiagnostics:{context}] error: {ex}");
        }

        return _setupDone;
    }

    // NotoSansCJKsc's baked SDF atlas texture isn't marked "Read/Write
    // Enabled", so TMP refuses to add new glyphs to it directly (even with
    // isMultiAtlasTexturesEnabled). Instead, create a brand new dynamic font
    // asset from our CJK TC TTF - CreateFontAsset allocates a fresh, readable
    // atlas texture - and register it as a fallback so missing
    // Traditional-only glyphs get rasterized there on demand.
    private static void SetupDynamicFallback(TMP_FontAsset target, string context)
    {
        _setupDone = true;
        PlaceholderFont ??= new Font(PlaceholderName);

        var dynamicFont = TMP_FontAsset.CreateFontAsset(PlaceholderFont);
        if (dynamicFont == null)
        {
            Plugin.Logger.LogError($"[FontDiagnostics:{context}] TMP_FontAsset.CreateFontAsset returned null");
            return;
        }

        dynamicFont.name = "NounTownZhTW_CJK_Dynamic";
        Plugin.Logger.LogInfo($"[FontDiagnostics:{context}] Created \"{dynamicFont.name}\": atlasPopulationMode={dynamicFont.atlasPopulationMode} sourceFontFile={(dynamicFont.sourceFontFile == null ? "null" : dynamicFont.sourceFontFile.name)} atlas={dynamicFont.atlasWidth}x{dynamicFont.atlasHeight} glyphCount={dynamicFont.glyphTable.Count} multiAtlas={dynamicFont.isMultiAtlasTexturesEnabled}");

        var fallbacks = target.fallbackFontAssetTable;
        if (fallbacks == null)
        {
            fallbacks = new Il2CppSystem.Collections.Generic.List<TMP_FontAsset>();
            target.fallbackFontAssetTable = fallbacks;
        }
        fallbacks.Add(dynamicFont);
        Plugin.Logger.LogInfo($"[FontDiagnostics:{context}] Added \"{dynamicFont.name}\" as fallback of \"{target.name}\" ({fallbacks.Count} entries)");

        var globalFallbacks = TMP_Settings.fallbackFontAssets;
        if (globalFallbacks != null)
        {
            globalFallbacks.Add(dynamicFont);
            Plugin.Logger.LogInfo($"[FontDiagnostics:{context}] Added \"{dynamicFont.name}\" to TMP_Settings.fallbackFontAssets ({globalFallbacks.Count} entries)");
        }

        RefreshAllText(context);
    }

    // Any TMP_Text that already rendered zh-TW glyphs before the dynamic CJK
    // fallback above was registered (e.g. text shown before the fallback
    // font asset finished loading) is stuck showing tofu for
    // Traditional-only characters: TMP only re-resolves glyphs when a text's
    // mesh is regenerated. Force that regeneration now so those
    // already-visible texts pick up the newly available fallback.
    private static void RefreshAllText(string context)
    {
        var texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        Plugin.Logger.LogInfo($"[FontDiagnostics:{context}] Forcing mesh update on {texts.Length} TMP_Text objects to pick up new CJK fallback");
        foreach (var t in texts)
        {
            t.ForceMeshUpdate(true, true);
        }
    }
}

[HarmonyPatch(typeof(FontEngine), nameof(FontEngine.LoadFontFace), new[] { typeof(Font), typeof(int), typeof(int) })]
internal static class Patch_LoadFontFace3
{
    static bool Prefix(Font font, int pointSize, int faceIndex, ref FontEngineError __result)
    {
        if (font != null && font.name == ZhTwFont.PlaceholderName && ZhTwFont.FontPath != null)
        {
            __result = FontEngine.LoadFontFace(ZhTwFont.FontPath, pointSize, faceIndex);
            Plugin.Logger.LogInfo($"[NounTownZhTW] LoadFontFace(placeholder, pointSize={pointSize}, faceIndex={faceIndex}) -> redirected to CJK TTF, result={__result}");
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(FontEngine), nameof(FontEngine.LoadFontFace), new[] { typeof(Font), typeof(int) })]
internal static class Patch_LoadFontFace2
{
    static bool Prefix(Font font, int pointSize, ref FontEngineError __result)
    {
        if (font != null && font.name == ZhTwFont.PlaceholderName && ZhTwFont.FontPath != null)
        {
            __result = FontEngine.LoadFontFace(ZhTwFont.FontPath, pointSize);
            Plugin.Logger.LogInfo($"[NounTownZhTW] LoadFontFace(placeholder, pointSize={pointSize}) -> redirected to CJK TTF, result={__result}");
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(FontEngine), nameof(FontEngine.LoadFontFace), new[] { typeof(Font) })]
internal static class Patch_LoadFontFace1
{
    static bool Prefix(Font font, ref FontEngineError __result)
    {
        if (font != null && font.name == ZhTwFont.PlaceholderName && ZhTwFont.FontPath != null)
        {
            __result = FontEngine.LoadFontFace(ZhTwFont.FontPath);
            Plugin.Logger.LogInfo($"[NounTownZhTW] LoadFontFace(placeholder) -> redirected to CJK TTF, result={__result}");
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(BundledObjectLoader), nameof(BundledObjectLoader.LoadTextFromAssets))]
internal static class Patch_LoadTextFromAssets
{
    static void Prefix(string assetName, string bundleName)
    {
        Plugin.Logger.LogInfo($"[BundledObjectLoader] LoadTextFromAssets(assetName=\"{assetName}\", bundleName=\"{bundleName}\")");
    }
}

[HarmonyPatch(typeof(BundledObjectLoader), nameof(BundledObjectLoader.CheckTextAssetExists))]
internal static class Patch_CheckTextAssetExists
{
    static bool Prefix(string assetName, string bundleName, ref bool __result)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            Plugin.Logger.LogInfo($"[BundledObjectLoader] CheckTextAssetExists(assetName=\"{assetName}\", bundleName=\"{bundleName}\") -> short-circuited (empty assetName)");
            __result = false;
            return false;
        }

        return true;
    }

    static void Postfix(string assetName, string bundleName, ref bool __result)
    {
        Plugin.Logger.LogInfo($"[BundledObjectLoader] CheckTextAssetExists(assetName=\"{assetName}\", bundleName=\"{bundleName}\") -> {__result}");
    }
}

[HarmonyPatch(typeof(BundledObjectLoader), nameof(BundledObjectLoader.LoadAudioClipFromAssets))]
internal static class Patch_LoadAudioClipFromAssets
{
    static void Prefix(string assetName, string bundleName)
    {
        Plugin.Logger.LogInfo($"[BundledObjectLoader] LoadAudioClipFromAssets(assetName=\"{assetName}\", bundleName=\"{bundleName}\")");
    }
}

[HarmonyPatch(typeof(BundledObjectLoader), "CheckLoadBundle")]
internal static class Patch_CheckLoadBundle
{
    private static readonly HashSet<string> RedirectedBundles = new()
    {
        "languagelistbundle", "localisationbundle", "languagedatabundle", "dialoguebundle",
    };

    private static readonly Dictionary<string, AssetBundle> ShadowBundles = new();

    static bool Prefix(string bundleName, ref AssetBundle __result)
    {
        Plugin.Logger.LogInfo($"[BundledObjectLoader] CheckLoadBundle(bundleName=\"{bundleName}\")");

        // CheckLoadBundle fires dozens of times per session from the very
        // start, so it's a reliable place to retry the dynamic CJK fallback
        // setup until the "NotoSansCJKsc" font asset has loaded (it isn't
        // available yet when Plugin.Load runs, and LanguageInit /
        // ChangeLanguageActions - the original setup triggers - don't run on
        // a normal launch without Settings interaction).
        ZhTwFont.TryFindAndSetup("CheckLoadBundle");

        if (!RedirectedBundles.Contains(bundleName))
        {
            return true;
        }

        if (!ShadowBundles.TryGetValue(bundleName, out var bundle) || bundle == null)
        {
            var path = Path.Combine(Plugin.ShadowDir, bundleName);
            bundle = AssetBundle.LoadFromFile(path);
            ShadowBundles[bundleName] = bundle;
            Plugin.Logger.LogInfo($"[NounTownZhTW] Loaded shadow bundle \"{bundleName}\" from {path} -> {(bundle == null ? "FAILED" : "OK")}");
        }

        __result = bundle!;
        return false;
    }
}

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.GetEditorLanguageName))]
internal static class Patch_GetEditorLanguageName
{
    static bool Prefix(string countryCode, ref string __result)
    {
        if (countryCode == "zh-TW")
        {
            __result = "chinese";
            return false;
        }

        return true;
    }
}

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.GetCountryCode))]
internal static class Patch_GetCountryCode
{
    static bool Prefix(string countryName, bool useDefault, ref string __result)
    {
        if (countryName == "Chinese (Traditional)")
        {
            __result = "zh-TW";
            return false;
        }

        // "chinese" is the shared GetEditorLanguageName() result for both
        // zh-CN and zh-TW, so the reverse lookup is ambiguous and the
        // original implementation always resolves it to "zh-CN". When the
        // zh-TW variant is the one actually in play (either because
        // ChangeLanguageActions is mid-call switching to/from it, or
        // because it's already the active/origin code), resolve to "zh-TW"
        // instead so bundle-asset selection (e.g. languagedatabundle "zh-tw")
        // follows the user's actual choice.
        if (countryName == "chinese"
            && (Patch_ChangeLanguageActions.PendingActive == "zh-TW"
                || Patch_ChangeLanguageActions.PendingOrigin == "zh-TW"
                || DataHandler.activeCode == "zh-TW"
                || DataHandler.originCode == "zh-TW"))
        {
            __result = "zh-TW";
            return false;
        }

        return true;
    }

    static void Postfix(string countryName, bool useDefault, string __result)
    {
        Plugin.Logger.LogInfo($"[DataHandler] GetCountryCode(\"{countryName}\", {useDefault}) -> \"{__result}\"");
        ZhTwFont.TryFindAndSetup("GetCountryCode");
    }
}

// The stock game's phonetic/pronunciation model data has no "zh-TW" entry -
// only "zh-CN". Now that the active language correctly resolves to "zh-TW"
// (see Patch_ParseLanguageListJson), code that sets up SRS test lists calls
// SetupPhoneticModel("zh-TW"), which throws a NullReferenceException deep in
// IL2CPP native code and crashes the game. Traditional and Simplified
// Chinese share the same Mandarin pronunciation, so use the "zh-CN" model
// for phonetics only - the on-screen text still comes from the zh-TW shadow
// bundles.
[HarmonyPatch(typeof(UniversalPhoneticUnitGenerator), nameof(UniversalPhoneticUnitGenerator.SetupPhoneticModel))]
internal static class Patch_SetupPhoneticModel
{
    static void Prefix(ref string language)
    {
        if (language == "zh-TW")
        {
            Plugin.Logger.LogInfo("[NounTownZhTW] SetupPhoneticModel(\"zh-TW\") -> using \"zh-CN\" phonetic model (no zh-TW model in stock game)");
            language = "zh-CN";
        }
    }
}

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.ParseLanguageListJson))]
internal static class Patch_ParseLanguageListJson
{
    static void Postfix()
    {
        Plugin.Logger.LogInfo($"[DataHandler] languageList.Count = {DataHandler.languageList.Count}");
        for (int i = 0; i < DataHandler.languageList.Count; i++)
        {
            var lang = DataHandler.languageList[i];
            var learning = lang.LearningDataList == null ? "null" : lang.LearningDataList.Count.ToString();
            var lesson = lang.LessonProgressDataList == null ? "null" : lang.LessonProgressDataList.Count.ToString();
            var streak = lang.StreakDataList == null ? "null" : lang.StreakDataList.Count.ToString();
            Plugin.Logger.LogInfo($"  [{i}] Id={lang.Id} CountryCode={lang.CountryCode} Name={lang.Name} EditorName={lang.EditorName} LearningDataList={learning} LessonProgressDataList={lesson} StreakDataList={streak}");
        }

        Plugin.Logger.LogInfo($"[DataHandler] activeCode={DataHandler.activeCode} activeName={DataHandler.activeName} originCode={DataHandler.originCode} originName={DataHandler.originName}");
        Plugin.Logger.LogInfo($"[DataHandler] bundleLanguageList={DataHandler.bundleLanguageList} bundleLanguageData={DataHandler.bundleLanguageData} bundleLocalisation={DataHandler.bundleLocalisation} bundleLesson={DataHandler.bundleLesson}");
        Plugin.Logger.LogInfo($"[DataHandler] bundleLanguageNeutralAudio={DataHandler.bundleLanguageNeutralAudio} bundleLanguageChatAudio={DataHandler.bundleLanguageChatAudio} bundleLanguageAmbientAudio={DataHandler.bundleLanguageAmbientAudio}");
        Plugin.Logger.LogInfo($"[DataHandler] bundleLanguageQuestionAudio={DataHandler.bundleLanguageQuestionAudio} bundleLanguageVerbAudio={DataHandler.bundleLanguageVerbAudio} bundleLanguageGameAudio={DataHandler.bundleLanguageGameAudio} bundleLanguageAudio={DataHandler.bundleLanguageAudio}");

        foreach (var code in new[] { "zh-CN", "en-US", "br-BZ", "es-MX", "ar-EG", "es-ES", "uk-UA" })
        {
            Plugin.Logger.LogInfo($"  GetEditorLanguageName(\"{code}\") = {DataHandler.GetEditorLanguageName(code)}");
        }

        // The save file stores the active/origin language as the editor name
        // "chinese", which is ambiguous between zh-CN and zh-TW. The game
        // always resolves that to "zh-CN" here, so on launch the saved zh-TW
        // choice gets silently reverted to zh-CN. Correct it using the
        // variant the user last explicitly selected (see
        // Patch_ChangeLanguageActions).
        if (DataHandler.activeName == "chinese" && DataHandler.activeCode != "zh-TW" && Plugin.ActiveChineseVariant.Value == "zh-TW")
        {
            DataHandler.activeCode = "zh-TW";
            Plugin.Logger.LogInfo("[NounTownZhTW] Corrected activeCode zh-CN -> zh-TW based on saved language preference");
        }
        if (DataHandler.originName == "chinese" && DataHandler.originCode != "zh-TW" && Plugin.OriginChineseVariant.Value == "zh-TW")
        {
            DataHandler.originCode = "zh-TW";
            Plugin.Logger.LogInfo("[NounTownZhTW] Corrected originCode zh-CN -> zh-TW based on saved language preference");
        }
    }
}

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.ParseLocalisationListJson))]
internal static class Patch_ParseLocalisationListJson
{
    static void Postfix()
    {
        Plugin.Logger.LogInfo($"[DataHandler] localisationList.Count = {DataHandler.localisationList.Count}");
        for (int i = 0; i < DataHandler.localisationList.Count && i < 3; i++)
        {
            var loc = DataHandler.localisationList[i];
            Plugin.Logger.LogInfo($"  [{i}] Id={loc.Id} Text={loc.Text}");
        }
    }
}

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.ParseItemList))]
internal static class Patch_ParseItemList
{
    static void Postfix()
    {
        Plugin.Logger.LogInfo($"[DataHandler] itemList.Count = {DataHandler.itemList.Count}");
        for (int i = 0; i < DataHandler.itemList.Count && i < 3; i++)
        {
            var item = DataHandler.itemList[i];
            var linked = item.LinkedItem == null ? "null" : item.LinkedItem.Count.ToString();
            var acceptable = item.AcceptableWords == null ? "null" : item.AcceptableWords.Count.ToString();
            Plugin.Logger.LogInfo($"  [{i}] Id={item.Id} EditorName={item.EditorName} OriginLanguageName={item.OriginLanguageName} OriginLanguagePhonetics={item.OriginLanguagePhonetics} ActiveLanguageName={item.ActiveLanguageName} ActiveLanguagePhonetics={item.ActiveLanguagePhonetics} Gender={item.Gender}");
            Plugin.Logger.LogInfo($"       ItemType={item.ItemType} SceneIn={item.SceneIn} SRS={item.SRS} Spawnable={item.Spawnable} SRError={item.SRError} LinkedItem.Count={linked} AcceptableWords.Count={acceptable}");
        }
    }
}

internal static class LanguageSelectorDiagnostics
{
    public static void Dump(string context, NTSettingsLanguageHandler instance)
    {
        Plugin.Logger.LogInfo($"[NTSettingsLanguageHandler] {context}");
        LogNames("originLanguageNames", instance.originLanguageNames);
        LogLanguages("originLanguageList", instance.originLanguageList);
        LogSelector("originLanguageSelector", instance.originLanguageSelector);
        LogNames("activeLanguageNames", instance.activeLanguageNames);
        LogLanguages("activeLanguageList", instance.activeLanguageList);
        LogSelector("activeLanguageSelector", instance.activeLanguageSelector);
    }

    private static void LogNames(string label, Il2CppSystem.Collections.Generic.List<string> list)
    {
        if (list == null) { Plugin.Logger.LogInfo($"  {label} = null"); return; }
        Plugin.Logger.LogInfo($"  {label}.Count = {list.Count}");
        for (int i = 0; i < list.Count; i++)
            Plugin.Logger.LogInfo($"    [{i}] = {list[i]}");
    }

    private static void LogLanguages(string label, Il2CppSystem.Collections.Generic.List<Language> list)
    {
        if (list == null) { Plugin.Logger.LogInfo($"  {label} = null"); return; }
        Plugin.Logger.LogInfo($"  {label}.Count = {list.Count}");
        for (int i = 0; i < list.Count; i++)
            Plugin.Logger.LogInfo($"    [{i}] CountryCode={list[i].CountryCode} EditorName={list[i].EditorName} Name={list[i].Name}");
    }

    private static void LogSelector(string label, Michsky.MUIP.HorizontalSelector selector)
    {
        if (selector == null) { Plugin.Logger.LogInfo($"  {label} = null"); return; }
        var items = selector.items;
        Plugin.Logger.LogInfo($"  {label}.items.Count = {(items == null ? -1 : items.Count)}, index={selector.index}, defaultIndex={selector.defaultIndex}");
        if (items == null)
            return;
        for (int i = 0; i < items.Count; i++)
            Plugin.Logger.LogInfo($"    [{i}] itemTitle={items[i].itemTitle}");
    }
}

[HarmonyPatch(typeof(NTSettingsLanguageHandler), "LanguageInit")]
internal static class Patch_LanguageInit
{
    static void Postfix(NTSettingsLanguageHandler __instance)
    {
        LanguageSelectorDiagnostics.Dump("after LanguageInit()", __instance);
        ZhTwFont.DumpAndSetup("LanguageInit");
    }
}

[HarmonyPatch(typeof(NTSettingsLanguageHandler), nameof(NTSettingsLanguageHandler.LanguagePanelInit))]
internal static class Patch_LanguagePanelInit
{
    static void Prefix(NTSettingsLanguageHandler __instance)
    {
        LanguageSelectorDiagnostics.Dump("before LanguagePanelInit()", __instance);
    }

    static void Postfix(NTSettingsLanguageHandler __instance)
    {
        LanguageSelectorDiagnostics.Dump("after LanguagePanelInit()", __instance);
    }
}

[HarmonyPatch(typeof(Michsky.MUIP.HorizontalSelector), nameof(Michsky.MUIP.HorizontalSelector.CreateNewItem), new[] { typeof(string) })]
internal static class Patch_CreateNewItem
{
    static void Postfix(Michsky.MUIP.HorizontalSelector __instance, string title)
    {
        Plugin.Logger.LogInfo($"[HorizontalSelector] CreateNewItem(title=\"{title}\") -> items.Count={__instance.items.Count}");
    }
}

[HarmonyPatch(typeof(Michsky.MUIP.HorizontalSelector), nameof(Michsky.MUIP.HorizontalSelector.RemoveItem))]
internal static class Patch_RemoveItem
{
    static void Postfix(Michsky.MUIP.HorizontalSelector __instance, string itemTitle)
    {
        Plugin.Logger.LogInfo($"[HorizontalSelector] RemoveItem(itemTitle=\"{itemTitle}\") -> items.Count={__instance.items.Count}");
    }
}

[HarmonyPatch(typeof(DataHandler), nameof(DataHandler.ChangeLanguageActions))]
internal static class Patch_ChangeLanguageActions
{
    // Captured for the duration of the call so Patch_GetCountryCode can
    // disambiguate "chinese" -> zh-CN vs zh-TW while this method is running.
    internal static string? PendingActive;
    internal static string? PendingOrigin;

    static void Prefix(string newActiveLanguageCode, string newOriginLanguageCode, bool shouldResetNsr)
    {
        PendingActive = newActiveLanguageCode;
        PendingOrigin = newOriginLanguageCode;
        Plugin.Logger.LogInfo($"[DataHandler] ChangeLanguageActions(newActive={newActiveLanguageCode}, newOrigin={newOriginLanguageCode}, resetNsr={shouldResetNsr})");

        // Remember which Chinese variant was explicitly chosen so the next
        // launch can resolve the save file's ambiguous "chinese" name to the
        // right country code (see Patch_ParseLanguageListJson).
        if (newActiveLanguageCode == "zh-CN" || newActiveLanguageCode == "zh-TW")
        {
            Plugin.ActiveChineseVariant.Value = newActiveLanguageCode;
        }
        if (newOriginLanguageCode == "zh-CN" || newOriginLanguageCode == "zh-TW")
        {
            Plugin.OriginChineseVariant.Value = newOriginLanguageCode;
        }
    }

    static void Postfix()
    {
        PendingActive = null;
        PendingOrigin = null;
        Plugin.Logger.LogInfo($"[DataHandler] after ChangeLanguageActions: activeCode={DataHandler.activeCode} activeName={DataHandler.activeName} originCode={DataHandler.originCode} originName={DataHandler.originName}");
        Plugin.Logger.LogInfo($"[DataHandler] bundleLanguageNeutralAudio={DataHandler.bundleLanguageNeutralAudio} bundleLanguageAudio={DataHandler.bundleLanguageAudio}");
        ZhTwFont.DumpAndSetup("ChangeLanguageActions");
    }
}
