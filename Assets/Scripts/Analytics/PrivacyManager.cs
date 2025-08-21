using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using CuriousCityAutomated.Analytics;

/// <summary>
/// Manages privacy compliance and data protection for learning analytics
/// </summary>
public class PrivacyManager : MonoBehaviour
{
    [Header("Privacy Settings")]
    public bool parentConsentGiven = false;
    public bool allowDataCollection = false;
    public bool allowDataExport = false;
    public bool anonymizeData = true;
    public bool enableDataDeletion = true;
    
    [Header("Data Retention")]
    public int dataRetentionDays = 365;
    public bool autoDeleteOldData = true;
    
    [Header("Consent Management")]
    public string parentEmail = "";
    public DateTime consentDate;
    public string consentVersion = "1.0";
    
    // Singleton instance
    private static PrivacyManager instance;
    public static PrivacyManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<PrivacyManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("PrivacyManager");
                    instance = go.AddComponent<PrivacyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    
    // Privacy state
    private string anonymousUserId;
    private Dictionary<string, bool> dataPermissions;
    private List<string> deletionQueue;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializePrivacy();
    }
    
    void InitializePrivacy()
    {
        LoadPrivacySettings();
        GenerateAnonymousId();
        
        dataPermissions = new Dictionary<string, bool>
        {
            ["behavioral_tracking"] = false,
            ["learning_analytics"] = false,
            ["progress_tracking"] = false,
            ["social_interactions"] = false,
            ["export_reports"] = false
        };
        
        deletionQueue = new List<string>();
        
        ValidatePrivacyCompliance();
    }
    
    void LoadPrivacySettings()
    {
        // Load from PlayerPrefs or secure storage
        parentConsentGiven = PlayerPrefs.GetInt("privacy_consent", 0) == 1;
        allowDataCollection = PlayerPrefs.GetInt("allow_collection", 0) == 1;
        allowDataExport = PlayerPrefs.GetInt("allow_export", 0) == 1;
        anonymizeData = PlayerPrefs.GetInt("anonymize_data", 1) == 1;
        parentEmail = PlayerPrefs.GetString("parent_email", "");
        
        string consentDateStr = PlayerPrefs.GetString("consent_date", "");
        if (!string.IsNullOrEmpty(consentDateStr))
        {
            DateTime.TryParse(consentDateStr, out consentDate);
        }
    }
    
    void SavePrivacySettings()
    {
        PlayerPrefs.SetInt("privacy_consent", parentConsentGiven ? 1 : 0);
        PlayerPrefs.SetInt("allow_collection", allowDataCollection ? 1 : 0);
        PlayerPrefs.SetInt("allow_export", allowDataExport ? 1 : 0);
        PlayerPrefs.SetInt("anonymize_data", anonymizeData ? 1 : 0);
        PlayerPrefs.SetString("parent_email", parentEmail);
        PlayerPrefs.SetString("consent_date", consentDate.ToString());
        PlayerPrefs.Save();
    }
    
    void GenerateAnonymousId()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("anonymous_id", "")))
        {
            anonymousUserId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("anonymous_id", anonymousUserId);
            PlayerPrefs.Save();
        }
        else
        {
            anonymousUserId = PlayerPrefs.GetString("anonymous_id");
        }
    }
    
    public void RequestParentConsent(string email, Action<bool> callback)
    {
        // In production, this would send a consent request email
        parentEmail = email;
        
        // For demo purposes, we'll simulate consent
        ShowConsentDialog((granted) =>
        {
            if (granted)
            {
                GrantConsent();
            }
            callback(granted);
        });
    }
    
    void ShowConsentDialog(Action<bool> callback)
    {
        // This would show a proper consent UI in production
        // For now, we'll simulate with a debug dialog
        Debug.Log("Parent consent requested for data collection and analysis");
        
        // Simulate consent granted
        callback(true);
    }
    
    public void GrantConsent()
    {
        parentConsentGiven = true;
        consentDate = DateTime.Now;
        allowDataCollection = true;
        
        // Update permissions
        dataPermissions["behavioral_tracking"] = true;
        dataPermissions["learning_analytics"] = true;
        dataPermissions["progress_tracking"] = true;
        dataPermissions["social_interactions"] = true;
        dataPermissions["export_reports"] = true;
        
        SavePrivacySettings();
        
        // Enable analytics systems
        EnableAnalyticsSystems();
    }
    
    public void RevokeConsent()
    {
        parentConsentGiven = false;
        allowDataCollection = false;
        allowDataExport = false;
        
        // Update permissions
        foreach (var key in dataPermissions.Keys)
        {
            dataPermissions[key] = false;
        }
        
        SavePrivacySettings();
        
        // Disable analytics and queue data deletion
        DisableAnalyticsSystems();
        QueueDataDeletion();
    }
    
    void EnableAnalyticsSystems()
    {
        var learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        if (learningTracker)
        {
            learningTracker.enableDetailedTracking = true;
        }
        
        var analyticsReporter = FindFirstObjectByType<ParentAnalyticsReporter>();
        if (analyticsReporter)
        {
            analyticsReporter.generateReportsAutomatically = true;
        }
    }
    
    void DisableAnalyticsSystems()
    {
        var learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        if (learningTracker)
        {
            learningTracker.enableDetailedTracking = false;
        }
        
        var analyticsReporter = FindFirstObjectByType<ParentAnalyticsReporter>();
        if (analyticsReporter)
        {
            analyticsReporter.generateReportsAutomatically = false;
        }
    }
    
    void QueueDataDeletion()
    {
        // Queue all analytics data for deletion
        string[] dataFiles = System.IO.Directory.GetFiles(Application.persistentDataPath, "*.json");
        deletionQueue.AddRange(dataFiles);
        
        // Process deletion
        ProcessDataDeletion();
    }
    
    void ProcessDataDeletion()
    {
        foreach (string file in deletionQueue)
        {
            try
            {
                System.IO.File.Delete(file);
                Debug.Log($"Deleted data file: {file}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete file: {e.Message}");
            }
        }
        
        deletionQueue.Clear();
    }
    
    public bool CanCollectData(string dataType)
    {
        if (!parentConsentGiven || !allowDataCollection)
            return false;
        
        return dataPermissions.ContainsKey(dataType) && dataPermissions[dataType];
    }
    
    public string AnonymizeIdentifier(string identifier)
    {
        if (!anonymizeData) return identifier;
        
        // Use SHA256 to create consistent anonymous IDs
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(identifier + anonymousUserId));
            return Convert.ToBase64String(hashBytes).Substring(0, 12);
        }
    }
    
    public Dictionary<string, object> AnonymizeData(Dictionary<string, object> data)
    {
        if (!anonymizeData) return data;
        
        var anonymized = new Dictionary<string, object>(data);
        
        // Remove or hash personally identifiable information
        string[] piiFields = { "player_name", "email", "ip_address", "device_id" };
        
        foreach (string field in piiFields)
        {
            if (anonymized.ContainsKey(field))
            {
                anonymized[field] = AnonymizeIdentifier(anonymized[field].ToString());
            }
        }
        
        return anonymized;
    }
    
    public void ValidatePrivacyCompliance()
    {
        // Check consent status
        if (!parentConsentGiven)
        {
            DisableAnalyticsSystems();
            Debug.Log("Analytics disabled: Parent consent required");
        }
        
        // Check data retention
        if (autoDeleteOldData)
        {
            CheckDataRetention();
        }
        
        // Validate email if provided
        if (!string.IsNullOrEmpty(parentEmail) && !IsValidEmail(parentEmail))
        {
            Debug.LogWarning("Invalid parent email format");
        }
    }
    
    void CheckDataRetention()
    {
        string analyticsPath = System.IO.Path.Combine(Application.persistentDataPath, "Analytics");
        if (!System.IO.Directory.Exists(analyticsPath)) return;
        
        string[] files = System.IO.Directory.GetFiles(analyticsPath, "*.json");
        DateTime cutoffDate = DateTime.Now.AddDays(-dataRetentionDays);
        
        foreach (string file in files)
        {
            var fileInfo = new System.IO.FileInfo(file);
            if (fileInfo.CreationTime < cutoffDate)
            {
                deletionQueue.Add(file);
            }
        }
        
        if (deletionQueue.Count > 0)
        {
            ProcessDataDeletion();
        }
    }
    
    bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    public string GetPrivacyPolicy()
    {
        return @"
Curious City Privacy Policy

Data Collection:
- We collect gameplay behavior data to provide personalized learning insights
- All data is encrypted and stored securely
- Personal information is anonymized by default

Parent Rights:
- Access your child's learning data at any time
- Export reports for educational purposes
- Delete all data upon request
- Revoke consent at any time

Data Usage:
- Improve educational content delivery
- Generate learning progress reports
- Adapt gameplay to learning styles
- Never sold to third parties

Contact: privacy@curiouscity.com
        ";
    }
    
    public void ExportPrivacyData(Action<string> callback)
    {
        if (!allowDataExport)
        {
            callback("Data export not permitted");
            return;
        }
        
        // Compile all user data for export
        var exportData = new Dictionary<string, object>
        {
            ["user_id"] = anonymousUserId,
            ["consent_date"] = consentDate.ToString(),
            ["data_collected"] = GetCollectedDataSummary(),
            ["privacy_settings"] = GetPrivacySettings()
        };
        
        string json = JsonUtility.ToJson(exportData, true);
        callback(json);
    }
    
    Dictionary<string, object> GetCollectedDataSummary()
    {
        // Summary of what data has been collected
        return new Dictionary<string, object>
        {
            ["sessions_recorded"] = PlayerPrefs.GetInt("total_sessions", 0),
            ["learning_events"] = PlayerPrefs.GetInt("total_learning_events", 0),
            ["data_size_mb"] = CalculateDataSize()
        };
    }
    
    Dictionary<string, bool> GetPrivacySettings()
    {
        return new Dictionary<string, bool>(dataPermissions);
    }
    
    float CalculateDataSize()
    {
        string dataPath = Application.persistentDataPath;
        var dirInfo = new System.IO.DirectoryInfo(dataPath);
        
        long totalSize = 0;
        foreach (var file in dirInfo.GetFiles("*.json", System.IO.SearchOption.AllDirectories))
        {
            totalSize += file.Length;
        }
        
        return totalSize / (1024f * 1024f); // Convert to MB
    }
}