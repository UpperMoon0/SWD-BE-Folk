﻿using GrowthTracking.DoctorSolution.Application.DTOs;
using GrowthTracking.ShareLibrary.Pagination;

namespace GrowthTracking.DoctorSolution.Application.Services.Interfaces
{
    public interface IDoctorService
    {
        Task<DoctorResponse> CreateDoctor(DoctorCreateRequest doctor);
        Task<DoctorResponse?> GetDoctorById(string doctorId);
        Task<PagedList<DoctorResponse>> GetAllDoctors(int page, int pageSize);
        Task<DoctorResponse> UpdateDoctor(DoctorUpdateRequest doctor);
        Task DeleteDoctor(string doctorId);
        Task<DoctorResponse> UpdateDoctorStatus(string doctorId, string newStatus);
        Task<PagedList<DoctorResponse>> SearchDoctors(string searchTerm, int page, int pageSize);

    }
}
