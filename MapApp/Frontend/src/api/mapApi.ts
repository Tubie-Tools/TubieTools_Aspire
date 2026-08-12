// api/mapApi.ts - API integration service
import axios from 'axios';
import {
  StateCapital,
  OptimizedRoute,
  TransportationPlan,
} from '../store/MapStore';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const mapApi = {
  // State Capitals
  getAllCapitals: async (): Promise<StateCapital[]> => {
    const response = await apiClient.get('/statecapitals');
    return response.data;
  },

  getCapitalByState: async (stateCode: string): Promise<StateCapital> => {
    const response = await apiClient.get(`/statecapitals/${stateCode}`);
    return response.data;
  },

  getCapitalsByRegion: async (region: string): Promise<StateCapital[]> => {
    const response = await apiClient.get(`/statecapitals/region/${region}`);
    return response.data;
  },

  getCapitalsWithSales: async (): Promise<StateCapital[]> => {
    const response = await apiClient.get('/statecapitals/sales/sold-to');
    return response.data;
  },

  getSalesStatistics: async (): Promise<any> => {
    const response = await apiClient.get('/statecapitals/sales/statistics');
    return response.data;
  },

  updateCapitalSales: async (
    stateCode: string,
    data: {
      hasSoldProducts: boolean;
      totalSalesAmount: number;
      productsSold: number;
      lastSaleDate?: string;
    }
  ): Promise<void> => {
    await apiClient.put(`/statecapitals/${stateCode}/sales`, data);
  },

  // Routes
  optimizeRoute: async (startingState: string): Promise<OptimizedRoute> => {
    const response = await apiClient.post('/routes/optimize', {
      startingState,
    });
    return response.data;
  },

  createTransportationPlan: async (
    startingState: string,
    vehicleCapacity: number = 10
  ): Promise<TransportationPlan> => {
    const response = await apiClient.post('/routes/transportation-plan', {
      startingState,
      vehicleCapacity,
    });
    return response.data;
  },

  getRoute: async (routeId: number): Promise<OptimizedRoute> => {
    const response = await apiClient.get(`/routes/${routeId}`);
    return response.data;
  },

  getAllRoutes: async (): Promise<OptimizedRoute[]> => {
    const response = await apiClient.get('/routes');
    return response.data;
  },

  getRouteSegments: async (routeId: number): Promise<any[]> => {
    const response = await apiClient.get(`/routes/${routeId}/segments`);
    return response.data;
  },

  calculateDistance: async (
    fromState: string,
    toState: string
  ): Promise<any> => {
    const response = await apiClient.post('/routes/distance', {
      fromState,
      toState,
    });
    return response.data;
  },
};
