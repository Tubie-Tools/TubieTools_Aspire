// App.tsx - Main application component
import React, { useState } from 'react';
import { Layout, Menu, Tabs, Alert } from 'antd';
import {
  EnvironmentOutlined,
  BarChartOutlined,
  RouteOutlined,
} from '@ant-design/icons';
import { Map } from './components/Map';
import { TransportationPlanner } from './components/TransportationPlanner';
import { SalesAnalytics } from './components/SalesAnalytics';
import { useMapStore } from './store/MapStore';
import './App.css';

const { Header, Content, Footer } = Layout;

function App() {
  const { error, setError } = useMapStore();
  const [activeTab, setActiveTab] = useState<string>('map');

  const tabItems = [
    {
      key: 'map',
      label: (
        <>
          <EnvironmentOutlined /> Map
        </>
      ),
      children: <Map />,
    },
    {
      key: 'transportation',
      label: (
        <>
          <RouteOutlined /> Transportation
        </>
      ),
      children: <TransportationPlanner />,
    },
    {
      key: 'analytics',
      label: (
        <>
          <BarChartOutlined /> Analytics
        </>
      ),
      children: <SalesAnalytics />,
    },
  ];

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header
        style={{
          display: 'flex',
          alignItems: 'center',
          backgroundColor: '#001529',
          color: '#fff',
          fontSize: '20px',
          fontWeight: 'bold',
        }}
      >
        <EnvironmentOutlined style={{ marginRight: '12px', fontSize: '24px' }} />
        US Map Application - State Capitals & Transportation
      </Header>

      <Content style={{ padding: '24px', backgroundColor: '#f5f5f5' }}>
        {error && (
          <Alert
            message="Error"
            description={error}
            type="error"
            closable
            onClose={() => setError(null)}
            style={{ marginBottom: '16px' }}
          />
        )}

        <Tabs
          activeKey={activeTab}
          onChange={setActiveTab}
          items={tabItems}
        />
      </Content>

      <Footer style={{ textAlign: 'center' }}>
        <p>Map Application ©2024 - Built for Transportation & Logistics Interview</p>
        <p style={{ fontSize: '12px', color: '#999' }}>
          Featuring 50 US State Capitals, Sales Tracking, and Intelligent Route Optimization
        </p>
      </Footer>
    </Layout>
  );
}

export default App;
