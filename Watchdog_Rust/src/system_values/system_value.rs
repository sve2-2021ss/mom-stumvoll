use crate::system_values::types::{CpuLoad, RamUsage, ServiceEvent};

#[derive(Debug)]
pub enum SystemValue {
    CpuLoadValue(CpuLoad),
    RamUsageValue(RamUsage),
    ServiceEventValue(ServiceEvent),
}