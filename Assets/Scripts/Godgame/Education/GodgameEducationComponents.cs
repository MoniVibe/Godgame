using PureDOTS.Runtime.Education;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Education
{
    /// <summary>
    /// Links elite families to their education policies.
    /// </summary>
    public struct EliteFamilyEducation : IComponentData
    {
        /// <summary>Family entity.</summary>
        public Entity Family;
        
        /// <summary>Current education policy ID.</summary>
        public ushort PolicyId;
        
        /// <summary>Total annual education budget.</summary>
        public float AnnualEducationBudget;
    }
    
    /// <summary>
    /// Court position assignment (from Elite Courts doc).
    /// </summary>
    public struct CourtPosition : IComponentData
    {
        /// <summary>Court position type (Champion, Steward, Wizard, etc.).</summary>
        public ushort PositionTypeId;
        
        /// <summary>Family/guild that employs this courtier.</summary>
        public Entity Employer;
        
        /// <summary>Courtier entity.</summary>
        public Entity Courtier;
        
        /// <summary>Annual salary.</summary>
        public float AnnualSalary;
        
        /// <summary>Employment start date.</summary>
        public float EmploymentDate;
    }
    
    /// <summary>
    /// Buffer for storing culturally transmitted lessons (replaces blob Entity array).
    /// </summary>
    [InternalBufferCapacity(16)]
    public struct StoredCulturalLesson : IBufferElementData
    {
        /// <summary>Lesson ID (references lesson catalog).</summary>
        public FixedString64Bytes LessonId;
        
        /// <summary>Lesson quality when stored.</summary>
        public byte Quality;
        
        /// <summary>Lesson rarity.</summary>
        public byte Rarity;
    }
}
















