using DivitageWinUI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DivitageWinUI.Services;

/// <summary>
/// 変換プロファイルの管理を行うサービスクラス
/// </summary>
public class ProfileService
{
    private readonly string _profilesPath;
    private List<ConversionProfile> _profiles = new();

    public ProfileService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Divitage"
        );
        Directory.CreateDirectory(appDataPath);
        _profilesPath = Path.Combine(appDataPath, "profiles.json");

        // デフォルトプロファイルを初期化
        InitializeDefaultProfiles();
    }

    /// <summary>
    /// デフォルトプロファイルを初期化します
    /// </summary>
    private void InitializeDefaultProfiles()
    {
        _profiles = new List<ConversionProfile>
        {
            new ConversionProfile
            {
                Name = "標準品質",
                Description = "バランスの取れた標準的な変換設定",
                Options = ConversionOptions.CreateDefault()
            },
            new ConversionProfile
            {
                Name = "高品質",
                Description = "高品質な変換設定（ファイルサイズが大きくなります）",
                Options = ConversionOptions.CreateHighQuality()
            },
            new ConversionProfile
            {
                Name = "高速変換",
                Description = "素早く変換（品質は低下します）",
                Options = ConversionOptions.CreateFast()
            },
            new ConversionProfile
            {
                Name = "Web最適化",
                Description = "Web配信に適した設定",
                Options = ConversionOptions.CreateWebOptimized()
            },
            new ConversionProfile
            {
                Name = "音声のみ",
                Description = "音声トラックのみを抽出",
                Options = new ConversionOptions
                {
                    OutputFormat = "mp3",
                    AudioCodec = "libmp3lame",
                    AudioBitrate = 192,
                    AudioOnly = true
                }
            }
        };
    }

    /// <summary>
    /// すべてのプロファイルを取得します
    /// </summary>
    public async Task<List<ConversionProfile>> GetAllProfilesAsync()
    {
        await LoadProfilesAsync();
        return _profiles.ToList();
    }

    /// <summary>
    /// プロファイルを追加します
    /// </summary>
    public async Task<ConversionProfile> AddProfileAsync(ConversionProfile profile)
    {
        profile.Id = Guid.NewGuid().ToString();
        profile.CreatedAt = DateTime.Now;
        profile.UpdatedAt = DateTime.Now;
        _profiles.Add(profile);
        await SaveProfilesAsync();
        return profile;
    }

    /// <summary>
    /// プロファイルを更新します
    /// </summary>
    public async Task UpdateProfileAsync(ConversionProfile profile)
    {
        var existing = _profiles.FirstOrDefault(p => p.Id == profile.Id);
        if (existing != null)
        {
            profile.UpdatedAt = DateTime.Now;
            var index = _profiles.IndexOf(existing);
            _profiles[index] = profile;
            await SaveProfilesAsync();
        }
    }

    /// <summary>
    /// プロファイルを削除します
    /// </summary>
    public async Task DeleteProfileAsync(string profileId)
    {
        var profile = _profiles.FirstOrDefault(p => p.Id == profileId);
        if (profile != null)
        {
            _profiles.Remove(profile);
            await SaveProfilesAsync();
        }
    }

    /// <summary>
    /// IDでプロファイルを取得します
    /// </summary>
    public async Task<ConversionProfile?> GetProfileByIdAsync(string profileId)
    {
        await LoadProfilesAsync();
        return _profiles.FirstOrDefault(p => p.Id == profileId);
    }

    /// <summary>
    /// プロファイルをファイルから読み込みます
    /// </summary>
    private async Task LoadProfilesAsync()
    {
        if (!File.Exists(_profilesPath))
        {
            await SaveProfilesAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_profilesPath);
            var loadedProfiles = JsonConvert.DeserializeObject<List<ConversionProfile>>(json);

            if (loadedProfiles != null && loadedProfiles.Any())
            {
                // デフォルトプロファイルと読み込んだプロファイルをマージ
                var customProfiles = loadedProfiles.Where(p =>
                    !_profiles.Any(dp => dp.Name == p.Name)).ToList();
                _profiles.AddRange(customProfiles);
            }
        }
        catch
        {
            // 読み込みに失敗した場合はデフォルトプロファイルを使用
        }
    }

    /// <summary>
    /// プロファイルをファイルに保存します
    /// </summary>
    private async Task SaveProfilesAsync()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_profiles, Formatting.Indented);
            await File.WriteAllTextAsync(_profilesPath, json);
        }
        catch
        {
            // 保存に失敗してもエラーにしない
        }
    }

    /// <summary>
    /// プロファイルをファイルからインポートします
    /// </summary>
    public async Task<ConversionProfile?> ImportProfileAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var profile = JsonConvert.DeserializeObject<ConversionProfile>(json);
            if (profile != null)
            {
                return await AddProfileAsync(profile);
            }
        }
        catch
        {
            // インポート失敗
        }
        return null;
    }

    /// <summary>
    /// プロファイルをファイルにエクスポートします
    /// </summary>
    public async Task<bool> ExportProfileAsync(string profileId, string filePath)
    {
        try
        {
            var profile = await GetProfileByIdAsync(profileId);
            if (profile != null)
            {
                var json = JsonConvert.SerializeObject(profile, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
        }
        catch
        {
            // エクスポート失敗
        }
        return false;
    }
}
