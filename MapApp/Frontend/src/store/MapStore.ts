// MapStore.ts - Zustand store for state management
import create from 'zustand';

export interface StateCapital {
  stateCode: string;
  stateName: string;
  capitalName: string;
  latitude: number;
  longitude: number;
  hasSoldProducts: boolean;
  lastSaleDate?: string;
  totalSalesAmount: number;
  productsSold: number;
  region: string;
  pinColor: string;
}

export interface RouteSegment {
  fromState: string;
  fromCapital: string;
  toState: string;
  toCapital: string;
  distanceKm: number;
  durationMinutes: number;
  latitude1: number;
  longitude1: number;
  latitude2: number;
  longitude2: number;
}

export interface OptimizedRoute {
  name: string;
  states: string[];
  stateNames: string[];
  totalDistanceKm: number;
  totalDurationMinutes: number;
  routeSegments: RouteSegment[];
  algorithm: string;
}

export interface TransportationPlan {
  startingState: string;
  routes: OptimizedRoute[];
  totalDistance: number;
  totalDurationHours: number;
  estimatedVehicles: number;
  createdAt: string;
}

interface MapStore {
  // State
  capitals: StateCapital[];
  selectedCapital: StateCapital | null;
  routes: OptimizedRoute[];
  transportationPlan: TransportationPlan | null;
  loading: boolean;
  error: string | null;

  // Actions
  setCapitals: (capitals: StateCapital[]) => void;
  setSelectedCapital: (capital: StateCapital | null) => void;
  setRoutes: (routes: OptimizedRoute[]) => void;
  setTransportationPlan: (plan: TransportationPlan | null) => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export const useMapStore = create<MapStore>((set) => ({
  capitals: [],
  selectedCapital: null,
  routes: [],
  transportationPlan: null,
  loading: false,
  error: null,

  setCapitals: (capitals) => set({ capitals }),
  setSelectedCapital: (capital) => set({ selectedCapital: capital }),
  setRoutes: (routes) => set({ routes }),
  setTransportationPlan: (plan) => set({ transportationPlan: plan }),
  setLoading: (loading) => set({ loading }),
  setError: (error) => set({ error }),
}));
