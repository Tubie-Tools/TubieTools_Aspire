// Map.tsx - Main map component using Leaflet
import React, { useEffect, useState } from 'react';
import {
  MapContainer,
  TileLayer,
  Marker,
  Popup,
  Polyline,
  Circle,
} from 'react-leaflet';
import L from 'leaflet';
import { Card, Row, Col, Spin, Select, Button, Space, Statistic } from 'antd';
import { EnvironmentOutlined, ShoppingOutlined } from '@ant-design/icons';
import { mapApi } from '../api/mapApi';
import { useMapStore, StateCapital, OptimizedRoute } from '../store/MapStore';
import '../styles/Map.css';

// Fix Leaflet icon issue
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/images/marker-icon-2x.png',
  iconUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/images/marker-icon.png',
  shadowUrl:
    'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.3.1/images/marker-shadow.png',
});

export const Map: React.FC = () => {
  const {
    capitals,
    selectedCapital,
    routes,
    setCapitals,
    setSelectedCapital,
    setLoading,
    setError,
  } = useMapStore();

  const [displayMode, setDisplayMode] = useState<'all' | 'sold' | 'region'>(
    'all'
  );
  const [selectedRegion, setSelectedRegion] = useState<string>('Northeast');
  const [showRoute, setShowRoute] = useState<boolean>(false);
  const [currentRoute, setCurrentRoute] = useState<OptimizedRoute | null>(null);
  const [salesStats, setSalesStats] = useState<any>(null);

  // Load data on component mount
  useEffect(() => {
    loadCapitals();
    loadSalesStatistics();
  }, []);

  const loadCapitals = async () => {
    try {
      setLoading(true);
      const data = await mapApi.getAllCapitals();
      setCapitals(data);
    } catch (error: any) {
      setError(error.message || 'Failed to load capitals');
    } finally {
      setLoading(false);
    }
  };

  const loadSalesStatistics = async () => {
    try {
      const stats = await mapApi.getSalesStatistics();
      setSalesStats(stats);
    } catch (error) {
      console.error('Failed to load sales statistics', error);
    }
  };

  const handleDisplayModeChange = async (mode: string) => {
    setDisplayMode(mode as 'all' | 'sold' | 'region');
    try {
      setLoading(true);
      if (mode === 'sold') {
        const data = await mapApi.getCapitalsWithSales();
        setCapitals(data);
      } else if (mode === 'region') {
        const data = await mapApi.getCapitalsByRegion(selectedRegion);
        setCapitals(data);
      } else {
        const data = await mapApi.getAllCapitals();
        setCapitals(data);
      }
    } catch (error: any) {
      setError(error.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleOptimizeRoute = async () => {
    if (!selectedCapital) {
      setError('Please select a starting capital');
      return;
    }

    try {
      setLoading(true);
      const route = await mapApi.optimizeRoute(selectedCapital.stateCode);
      setCurrentRoute(route);
      setShowRoute(true);
    } catch (error: any) {
      setError(error.message || 'Failed to optimize route');
    } finally {
      setLoading(false);
    }
  };

  const getDisplayCapitals = (): StateCapital[] => {
    if (displayMode === 'sold') {
      return capitals.filter((c) => c.hasSoldProducts);
    } else if (displayMode === 'region') {
      return capitals.filter((c) => c.region === selectedRegion);
    }
    return capitals;
  };

  const displayCapitals = getDisplayCapitals();

  // Create route polyline coordinates
  const routeCoordinates =
    currentRoute?.routeSegments.map((seg) => [
      [seg.latitude1, seg.longitude1],
      [seg.latitude2, seg.longitude2],
    ]) || [];

  const center = [39.8283, -98.5795] as [number, number]; // Center of USA

  return (
    <div className="map-container">
      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total States"
              value={salesStats?.totalStates || 0}
              prefix={<EnvironmentOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="States with Sales"
              value={salesStats?.statesWithSales || 0}
              valueStyle={{ color: '#ff4d4f' }}
              prefix={<ShoppingOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Total Sales"
              value={salesStats?.totalSalesAmount || 0}
              prefix="$"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="Products Sold"
              value={salesStats?.totalProductsSold || 0}
            />
          </Card>
        </Col>
      </Row>

      <Row gutter={[16, 16]} style={{ marginBottom: '16px' }}>
        <Col xs={24} md={6}>
          <Card title="Controls" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <Select
                value={displayMode}
                onChange={handleDisplayModeChange}
                style={{ width: '100%' }}
                options={[
                  { label: 'All Capitals', value: 'all' },
                  { label: 'Sold To', value: 'sold' },
                  { label: 'By Region', value: 'region' },
                ]}
              />

              {displayMode === 'region' && (
                <Select
                  value={selectedRegion}
                  onChange={(value) => {
                    setSelectedRegion(value);
                    handleDisplayModeChange('region');
                  }}
                  style={{ width: '100%' }}
                  options={[
                    { label: 'Northeast', value: 'Northeast' },
                    { label: 'Southeast', value: 'Southeast' },
                    { label: 'Midwest', value: 'Midwest' },
                    { label: 'Southwest', value: 'Southwest' },
                    { label: 'West', value: 'West' },
                    { label: 'South', value: 'South' },
                  ]}
                />
              )}

              <Button
                type="primary"
                onClick={handleOptimizeRoute}
                disabled={!selectedCapital}
                block
              >
                Optimize Route
              </Button>

              {selectedCapital && (
                <div style={{ fontSize: '12px', color: '#999' }}>
                  From: <strong>{selectedCapital.capitalName}</strong>
                </div>
              )}
            </Space>
          </Card>
        </Col>

        <Col xs={24} md={18}>
          <Card title="US State Capitals Map" loading={false}>
            <Spin spinning={displayCapitals.length === 0}>
              <MapContainer
                center={center}
                zoom={4}
                style={{ height: '600px', borderRadius: '4px' }}
              >
                <TileLayer
                  attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                  url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                />

                {/* Markers for all capitals */}
                {displayCapitals.map((capital) => (
                  <Marker
                    key={capital.stateCode}
                    position={[capital.latitude, capital.longitude]}
                    eventHandlers={{
                      click: () => setSelectedCapital(capital),
                    }}
                    icon={L.icon({
                      iconUrl: `data:image/svg+xml;base64,${btoa(
                        `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="${capital.pinColor}"><path d="M12 0C7.58 0 4 3.58 4 8c0 5.25 8 16 8 16s8-10.75 8-16c0-4.42-3.58-8-8-8zm0 12c-2.21 0-4-1.79-4-4s1.79-4 4-4 4 1.79 4 4-1.79 4-4 4z"/></svg>`
                      )}`,
                      iconSize: [32, 32],
                      iconAnchor: [16, 32],
                      popupAnchor: [0, -32],
                    })}
                  >
                    <Popup>
                      <div style={{ minWidth: '250px' }}>
                        <h4>{capital.capitalName}</h4>
                        <p>
                          <strong>State:</strong> {capital.stateName}
                        </p>
                        <p>
                          <strong>Region:</strong> {capital.region}
                        </p>
                        <p>
                          <strong>Sales:</strong>{' '}
                          {capital.hasSoldProducts ? (
                            <span style={{ color: 'green' }}>✓ Yes</span>
                          ) : (
                            <span style={{ color: 'red' }}>✗ No</span>
                          )}
                        </p>
                        {capital.hasSoldProducts && (
                          <>
                            <p>
                              <strong>Total Sales:</strong> ${capital.totalSalesAmount}
                            </p>
                            <p>
                              <strong>Products Sold:</strong>{' '}
                              {capital.productsSold}
                            </p>
                          </>
                        )}
                      </div>
                    </Popup>
                  </Marker>
                ))}

                {/* Route polylines */}
                {showRoute &&
                  currentRoute?.routeSegments.map((segment, idx) => (
                    <Polyline
                      key={idx}
                      positions={[
                        [segment.latitude1, segment.longitude1],
                        [segment.latitude2, segment.longitude2],
                      ]}
                      color="#0066cc"
                      weight={2}
                      opacity={0.7}
                    />
                  ))}
              </MapContainer>
            </Spin>
          </Card>
        </Col>
      </Row>

      {currentRoute && showRoute && (
        <Card title="Route Details" style={{ marginTop: '16px' }}>
          <Row gutter={16}>
            <Col xs={24} sm={12} md={4}>
              <Statistic
                title="States"
                value={currentRoute.states.length}
              />
            </Col>
            <Col xs={24} sm={12} md={4}>
              <Statistic
                title="Distance"
                value={currentRoute.totalDistanceKm.toFixed(0)}
                suffix="km"
              />
            </Col>
            <Col xs={24} sm={12} md={4}>
              <Statistic
                title="Duration"
                value={(currentRoute.totalDurationMinutes / 60).toFixed(1)}
                suffix="hours"
              />
            </Col>
            <Col xs={24} sm={12} md={12}>
              <strong>Route Order:</strong>{' '}
              {currentRoute.stateNames.join(' → ')}
            </Col>
          </Row>
        </Card>
      )}
    </div>
  );
};
