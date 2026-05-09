using System;
using System.Collections.Generic;

namespace SmartClassRoom.Web.TempModels;

public partial class Material
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string FilePath { get; set; } = null!;

    public string? FileType { get; set; }

    public long FileSize { get; set; }

    public int CourseOfferingId { get; set; }

    public int? FolderId { get; set; }

    public int UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public int? DownloadCount { get; set; }

    public bool IsActive { get; set; }

    public int UploadedByTeacherId { get; set; }

    public virtual CourseOffering CourseOffering { get; set; } = null!;

    public virtual MaterialFolder? Folder { get; set; }

    public virtual User UploadedByTeacher { get; set; } = null!;
}
