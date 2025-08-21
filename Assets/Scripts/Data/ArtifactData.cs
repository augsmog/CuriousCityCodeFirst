using System;
using System.Collections.Generic;
using UnityEngine;

namespace CuriousCity.Data
{
    /// <summary>
    /// Data structure for artifact information used throughout the game
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New", menuName = "CuriousCity/Data")]
public class ArtifactData
    {
        [Header("Basic Information")]
        public string id = "artifact_001";
        public string artifactName = "Ancient Artifact";
        public string description = "A mysterious artifact from the past";
        public Sprite icon;
        
        [Header("Gameplay Properties")]
        public int artifactPower = 10;
        public string systemBenefit = "Increases ship power efficiency";
        public ArtifactType artifactType = ArtifactType.Power;
        
        [Header("Visual Properties")]
        public Color glowColor = Color.yellow;
        public float glowIntensity = 2f;
        public GameObject visualPrefab;
        
        [Header("Collection Requirements")]
        public List<string> requiredItems = new List<string>();
        public List<string> requiredPuzzles = new List<string>();
        public int requiredLevel = 0;
        
        [Header("System Effects")]
        public float moraleBonus = 0f;
        public float powerBonus = 0f;
        public float populationBonus = 0f;
        public float colonyProgressBonus = 0f;
        
        [Header("Story Integration")]
        public List<string> unlocksDialogues = new List<string>();
        public List<string> unlocksFeatures = new List<string>();
        public string loreText = "";
        
        // Compatibility properties (for scripts expecting different property names)
        public string name 
        { 
            get { return artifactName; }
            set { artifactName = value; }
        }
        
        public string type
        {
            get { return artifactType.ToString(); }
            set 
            { 
                if (Enum.TryParse<ArtifactType>(value, true, out ArtifactType parsedType))
                {
                    artifactType = parsedType;
                }
            }
        }
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public ArtifactData()
        {
            requiredItems = new List<string>();
            requiredPuzzles = new List<string>();
            unlocksDialogues = new List<string>();
            unlocksFeatures = new List<string>();
        }
        
        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public ArtifactData(string name, string type, string desc, string benefit)
        {
            artifactName = name;
            description = desc;
            systemBenefit = benefit;
            
            // Initialize lists
            requiredItems = new List<string>();
            requiredPuzzles = new List<string>();
            unlocksDialogues = new List<string>();
            unlocksFeatures = new List<string>();
            
            // Parse type
            if (Enum.TryParse<ArtifactType>(type, true, out ArtifactType parsedType))
            {
                artifactType = parsedType;
            }
        }
        
        /// <summary>
        /// Calculate the total system improvement this artifact provides
        /// </summary>
        public float GetTotalSystemImprovement()
        {
            return moraleBonus + powerBonus + populationBonus + colonyProgressBonus;
        }
        
        /// <summary>
        /// Check if artifact has any unlock features
        /// </summary>
        public bool HasUnlocks()
        {
            return unlocksDialogues.Count > 0 || unlocksFeatures.Count > 0;
        }
        
        /// <summary>
        /// Get a formatted display name for UI
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(artifactName) ? "Unknown Artifact" : artifactName;
        }
        
        /// <summary>
        /// Get a shortened description for tooltips
        /// </summary>
        public string GetShortDescription(int maxLength = 50)
        {
            if (string.IsNullOrEmpty(description))
                return "No description available.";
                
            if (description.Length <= maxLength)
                return description;
                
            return description.Substring(0, maxLength - 3) + "...";
        }
        
        /// <summary>
        /// Check if this artifact meets collection requirements
        /// </summary>
        public bool CanCollect(int playerLevel, List<string> playerItems, List<string> completedPuzzles)
        {
            // Check level requirement
            if (playerLevel < requiredLevel)
                return false;
                
            // Check required items
            foreach (var item in requiredItems)
            {
                if (!playerItems.Contains(item))
                    return false;
            }
            
            // Check required puzzles
            foreach (var puzzle in requiredPuzzles)
            {
                if (!completedPuzzles.Contains(puzzle))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Apply artifact effects to system
        /// </summary>
        public void ApplyEffects(ref float morale, ref float power, ref float population, ref float progress)
        {
            morale += moraleBonus;
            power += powerBonus;
            population += populationBonus;
            progress += colonyProgressBonus;
        }
        
        /// <summary>
        /// Clone this artifact data
        /// </summary>
        public ArtifactData Clone()
        {
            var clone = new ArtifactData
            {
                id = this.id,
                artifactName = this.artifactName,
                description = this.description,
                icon = this.icon,
                artifactPower = this.artifactPower,
                systemBenefit = this.systemBenefit,
                artifactType = this.artifactType,
                glowColor = this.glowColor,
                glowIntensity = this.glowIntensity,
                visualPrefab = this.visualPrefab,
                requiredLevel = this.requiredLevel,
                moraleBonus = this.moraleBonus,
                powerBonus = this.powerBonus,
                populationBonus = this.populationBonus,
                colonyProgressBonus = this.colonyProgressBonus,
                loreText = this.loreText
            };
            
            // Clone lists
            clone.requiredItems = new List<string>(this.requiredItems);
            clone.requiredPuzzles = new List<string>(this.requiredPuzzles);
            clone.unlocksDialogues = new List<string>(this.unlocksDialogues);
            clone.unlocksFeatures = new List<string>(this.unlocksFeatures);
            
            return clone;
        }
        
        public override string ToString()
        {
            return $"Artifact: {artifactName} (Type: {artifactType}, Power: {artifactPower})";
        }
    }

    /// <summary>
    /// Types of artifacts that can be collected
    /// </summary>
    public enum ArtifactType
    {
        Power,
        Morale,
        Population,
        Technology,
        Cultural,
        Scientific,
        Agricultural,
        Medical,
        Educational,
        Universal
    }
    
    /// <summary>
    /// Artifact collection event data
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "New", menuName = "CuriousCity/Data")]
public class ArtifactCollectionEvent
    {
        public string artifactId;
        public DateTime collectionTime;
        public Vector3 collectionLocation;
        public float timeTakenToCollect;
        public int hintsUsedBeforeCollection;
        
        public ArtifactCollectionEvent(string id)
        {
            artifactId = id;
            collectionTime = DateTime.Now;
        }
    }
}