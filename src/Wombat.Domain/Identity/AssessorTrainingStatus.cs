namespace Wombat.Domain.Identity;

/// <summary>
/// Lifecycle of an assessor's faculty-development / assessor-training status.
/// The optional <see cref="AssessorProfile.TrainingCompletedOn"/> date is a
/// sub-field that is meaningful once the assessor reaches
/// <see cref="Provisional"/> or <see cref="Trained"/>.
/// </summary>
public enum AssessorTrainingStatus
{
    /// <summary>Profiled but assessor training has not begun (default; legacy "blank date").</summary>
    NotStarted = 0,

    /// <summary>Actively undergoing assessor training; may assess under supervision.</summary>
    InTraining = 1,

    /// <summary>Training completed but pending faculty sign-off / certification.</summary>
    Provisional = 2,

    /// <summary>Fully certified assessor.</summary>
    Trained = 3,
}
