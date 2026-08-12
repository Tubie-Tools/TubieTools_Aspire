// SalesAnalytics.tsx - Component for sales analytics
import React, { useEffect, useState } from 'react';
import {
  Card,
  Row,
  Col,
  Spin,
  Table,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'antd';
import { mapApi } from '../api/mapApi';

export const SalesAnalytics: React.FC = () => {
  const [stats, setStats] = useState<any>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadStatistics();
  }, []);

  const loadStatistics = async () => {
    try {
      setLoading(true);
      const data = await mapApi.getSalesStatistics();
      setStats(data);
    } catch (error) {
      console.error('Failed to load statistics', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return <Spin />;
  }

  if (!stats) {
    return <div>No data available</div>;
  }

  const columns = [
    {
      title: 'State Code',
      dataIndex: 'stateCode',
      key: 'stateCode',
    },
    {
      title: 'State Name',
      dataIndex: 'stateName',
      key: 'stateName',
    },
    {
      title: 'Total Sales',
      dataIndex: 'totalSales',
      key: 'totalSales',
      render: (text: number) => `$${text.toFixed(2)}`,
    },
    {
      title: 'Products Sold',
      dataIndex: 'productsSold',
      key: 'productsSold',
    },
  ];

  const chartData = stats.topSellingStates.map((state: any) => ({
    name: state.stateCode,
    sales: parseFloat(state.totalSales),
  }));

  return (
    <div style={{ padding: '24px' }}>
      <Row gutter={[16, 16]} style={{ marginBottom: '24px' }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <div>
              <strong>Total States</strong>
              <div style={{ fontSize: '28px', color: '#1890ff' }}>
                {stats.totalStates}
              </div>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <div>
              <strong>States with Sales</strong>
              <div style={{ fontSize: '28px', color: '#52c41a' }}>
                {stats.statesWithSales}
              </div>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <div>
              <strong>Total Sales</strong>
              <div style={{ fontSize: '24px', color: '#faad14' }}>
                ${stats.totalSalesAmount.toFixed(0)}
              </div>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <div>
              <strong>Products Sold</strong>
              <div style={{ fontSize: '28px', color: '#ff4d4f' }}>
                {stats.totalProductsSold}
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      <Card title="Top Selling States" style={{ marginBottom: '24px' }}>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="name" />
            <YAxis />
            <Tooltip formatter={(value: number) => `$${value.toFixed(0)}`} />
            <Legend />
            <Bar dataKey="sales" fill="#1890ff" name="Sales ($)" />
          </BarChart>
        </ResponsiveContainer>
      </Card>

      <Card title="All Top Selling States">
        <Table
          dataSource={stats.topSellingStates.map((state: any, idx: number) => ({
            ...state,
            key: idx,
          }))}
          columns={columns}
          pagination={{ pageSize: 10 }}
        />
      </Card>
    </div>
  );
};
