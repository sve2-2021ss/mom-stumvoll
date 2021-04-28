use anyhow::{anyhow, Result};
use prost::Message;

use crate::system_values::system_value::SystemValue;
use crate::system_values::system_value::SystemValue::{CpuLoadValue, RamUsageValue, ServiceEventValue};
use crate::system_values::types::{CpuLoad, RamUsage, ServiceEvent};

pub fn parse_data(key: &str, data: Vec<u8>) -> Result<SystemValue> {
    if key.contains("cpu.load") {
        let cpu_load = CpuLoad::decode(data.as_slice())?;
        return Ok(CpuLoadValue(cpu_load));
    }
    if key.contains("ram.usage") {
        let ram_usage = RamUsage::decode(data.as_slice())?;
        return Ok(RamUsageValue(ram_usage));
    }
    if key.contains("service.event") {
        let service_event = ServiceEvent::decode(data.as_slice())?;
        return Ok(ServiceEventValue(service_event));
    }

    Err(anyhow!("No matching key for {}", key))
}