// TransportationPlanner.tsx - Component for creating transportation plans
import React, { useState } from 'react';
import {
  Card,
  Row,
  Col,
  Button,
  Select,
  Spin,
  Statistic,
  Table,
  Space,
  Tabs,
} from 'antd';
import {
  RouteOutlined,
  TruckOutlined,
  EnvironmentOutlined,
} from '@ant-design/icons';
import { mapApi } from '../api/mapApi';
import { useMapStore, TransportationPlan, OptimizedRoute } from '../store/MapStore';

export const TransportationPlanner: React.FC = () => {
  const { capitals, setLoading } = useMapStore();
  const [selectedStartState, setSelectedStartState] = useState<string | null>(
    null
  );
  const [vehicleCapacity, setVehicleCapacity] = useState<number>(10);
  const [transportationPlan, setTransportationPlan] =
    useState<TransportationPlan | null>(null);
  const [loading, setLoadingLocal] = useState(false);

  const handleCreatePlan = async () => {
    if (!selectedStartState) {
      alert('Please select a starting state');
      return;
    }

    try {
      setLoadingLocal(true);
      const plan = await mapApi.createTransportationPlan(
        selectedStartState,
        vehicleCapacity
      );
      setTransportationPlan(plan);
    } catch (error: any) {
      alert('Failed to create transportation plan: ' + error.message);
    } finally {
      setLoadingLocal(false);
    }
  };

  const stateOptions = capitals.map((c) => ({
    label: `${c.stateName} (${c.stateCode})`,
    value: c.stateCode,
  }));

  return (
    <div style={{ padding: '24px' }}>
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} md={6}>
          <Card title="Plan Configuration" size="small">
            <Space direction="vertical" style={{ width: '100%' }}>
              <div>
                <label>Starting State:</label>
                <Select
                  value={selectedStartState}
                  onChange={setSelectedStartState}
                  placeholder="Select starting state"
                  style={{ width: '100%' }}
                  options={stateOptions}
                />
              </div>

              <div>
                <label>Vehicle Capacity:</label>
                <Select
                  value={vehicleCapacity}
                  onChange={setVehicleCapacity}
                  style={{ width: '100%' }}
                  options={[
                    { label: '5 States', value: 5 },
                    { label: '10 States', value: 10 },
                    { label: '15 States', value: 15 },
                    { label: '20 States', value: 20 },
                  ]}
                />
              </div>

              <Button
                type="primary"
                onClick={handleCreatePlan}
                loading={loading}
                block
                icon={<RouteOutlined />}
              >
                Create Plan
              </Button>
            </Space>
          </Card>
        </Col>

        {transportationPlan && (
          <>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Routes"
                  value={transportationPlan.routes.length}
                  prefix={<RouteOutlined />}
                  valueStyle={{ color: '#1890ff' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Total Distance"
                  value={transportationPlan.totalDistance.toFixed(0)}
                  suffix="km"
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="Est. Duration"
                  value={transportationPlan.totalDurationHours}
                  suffix="hours"
                />
              </Card>
            </Col>
          </>
        )}
      </Row>

      {transportationPlan && (
        <Card title="Transportation Routes">
          <Tabs
            items={transportationPlan.routes.map((route, idx) => ({
              key: idx.toString(),
              label: `Route ${idx + 1} - ${route.states.length} states`,
              children: <RouteDetail route={route} index={idx} />,
            }))}
          />
        </Card>
      )}
    </div>
  );
};

interface RouteDetailProps {
  route: OptimizedRoute;
  index: number;
}

const RouteDetail: React.FC<RouteDetailProps> = ({ route, index }) => {
  const routeColumns = [
    {
      title: 'From',
      dataIndex: ['fromCapital'],
      key: 'fromCapital',
    },
    {
      title: 'To',
      dataIndex: ['toCapital'],
      key: 'toCapital',
    },
    {
      title: 'Distance (km)',
      dataIndex: 'distanceKm',
      key: 'distanceKm',
      render: (text: number) => text.toFixed(1),
    },
    {
      title: 'Duration (min)',
      dataIndex: 'durationMinutes',
      key: 'durationMinutes',
    },
  ];

  return (
    <div>
      <Row gutter={16} style={{ marginBottom: '16px' }}>
        <Col xs={24} sm={12} md={6}>
          <Statistic
            title="States in Route"
            value={route.states.length}
            prefix={<EnvironmentOutlined />}
          />
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Statistic
            title="Total Distance"
            value={route.totalDistanceKm.toFixed(0)}
            suffix="km"
          />
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Statistic
            title="Est. Duration"
            value={(route.totalDurationMinutes / 60).toFixed(1)}
            suffix="hours"
          />
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Statistic
            title="Algorithm"
            value={route.algorithm}
          />
        </Col>
      </Row>

      <div style={{ marginBottom: '16px' }}>
        <strong>Route Order:</strong>
        <p
          style={{
            color: '#666',
            fontSize: '12px',
            wordBreak: 'break-all',
            marginTop: '8px',
          }}
        >
          {route.stateNames.join(' → ')}
        </p>
      </div>

      <Table
        dataSource={route.routeSegments.map((seg, idx) => ({
          ...seg,
          key: idx,
        }))}
        columns={routeColumns}
        size="small"
        pagination={false}
      />
    </div>
  );
};
