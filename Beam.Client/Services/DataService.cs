﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beam.Shared;
using Microsoft.AspNetCore.Components;

namespace Beam.Client.Services
{
  public class DataService : IDataService
  {
    public IReadOnlyList<Frequency> Frequencies { get; private set; }
    public IReadOnlyList<Ray> Rays { get; private set; } = new List<Ray>();
    public User CurrentUser { get; set; }
    private int? selectedFrequency;
    private IBeamApiService _apiService;
    private readonly NavigationManager _navigationManager;

    public int SelectedFrequency
    {
      get
      {
        if (!selectedFrequency.HasValue && Frequencies.Count > 0)
        {
          selectedFrequency = Frequencies.First().Id;
        }
        return selectedFrequency ?? 0;
      }
      set
      {
        selectedFrequency = value;
        GetRays(value).ConfigureAwait(false);
      }
    }

    public DataService(IBeamApiService apiService, NavigationManager navigationManager)
    {
      _apiService = apiService;
      _navigationManager = navigationManager;
      if (CurrentUser == null) CurrentUser = new User() { Name = "Anon" + new Random().Next(0, 10) };
    }

    public event Action UpdatedFrequencies;
    public event Action UpdatedRays;

    public async Task GetFrequencies()
    {
      Frequencies = await _apiService.FrequencyList();
      UpdatedFrequencies?.Invoke();
    }

    public async Task GetRays(int FrequencyId)
    {
      Rays = new List<Ray>();
      Rays = await _apiService.RayList(FrequencyId);
      UpdatedRays?.Invoke();
    }

    public async Task<List<Ray>> GetUserRays(string name)
    {
      return await _apiService.UserRays(name ?? CurrentUser.Name);
    }

    public async Task AddFrequency(string Name)
    {
      Frequencies = await _apiService.AddFrequency(new Frequency() { Name = Name });
      UpdatedFrequencies?.Invoke();
    }

    public async Task CreateRay(string text)
    {
      var ray = new Ray()
      {
        FrequencyId = selectedFrequency.Value,
        Text = text,
        UserId = CurrentUser.Id
      };

      if (CurrentUser.Id == 0)
      {
        _navigationManager.NavigateTo("/login");
        ray.UserId = CurrentUser.Id;
      }

      Rays = await _apiService.AddRay(ray);
      UpdatedRays?.Invoke();
    }

    public async Task PrismRay(int RayId)
    {
      if (CurrentUser.Id == 0) _navigationManager.NavigateTo("/login");

      Rays = await _apiService.PrismRay(new Prism() { RayId = RayId, UserId = CurrentUser.Id });
      UpdatedRays?.Invoke();
    }

    public async Task UnPrismRay(int RayId)
    {
      if (CurrentUser.Id == 0)  _navigationManager.NavigateTo("/login");

      Rays = await _apiService.UnPrismRay(RayId, CurrentUser.Id);
      UpdatedRays?.Invoke();
    }
  }
}