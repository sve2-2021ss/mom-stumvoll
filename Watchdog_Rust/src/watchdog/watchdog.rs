use colour::red_ln;

use crate::config::config::Config;
use crate::system_values::system_value::SystemValue;
use crate::system_values::types::{CpuLoad, RamUsage, ServiceEvent, ServiceEventType};

pub struct Watchdog {
    config: Box<Config>,
}

impl Watchdog {
    pub fn new(config: Box<Config>) -> Watchdog {
        Watchdog { config }
    }

    pub fn on_system_value(&self, device_name: &str, system_value: &SystemValue) {
        match system_value {
            SystemValue::CpuLoadValue(cpu) if self.check_cpu_load(cpu) =>
                Self::print_error(
                    device_name,
                    &format!("CPU load is {}% (Threshold {}%)", cpu.load, self.config.cpu_max),
                ),
            SystemValue::RamUsageValue(ram) if self.check_ram_usage(ram) =>
                Self::print_error(
                    device_name,
                    &format!("Ram usage is {}mb out of {}mb (Threshold {}%)", ram.used_mb, ram.total_mb, self.config.cpu_max),
                ),
            SystemValue::ServiceEventValue(svc) if self.check_service_event(svc) => Self::print_error(
                device_name,
                &format!("Critical process {} crashed", svc.executable),
            ),
            _ => {}
        }
    }

    fn check_cpu_load(&self, cpu_load: &CpuLoad) -> bool {
        cpu_load.load > self.config.cpu_max
    }

    fn check_ram_usage(&self, ram_usage: &RamUsage) -> bool {
        (100 * ram_usage.used_mb / ram_usage.total_mb) > self.config.cpu_max
    }

    fn check_service_event(&self, service_event: &ServiceEvent) -> bool {
        let is_stop = ServiceEventType::from_i32(service_event.service_event_type)
            .filter(|v| v == &ServiceEventType::Stop)
            .is_some();

        self.config.delicate_processes.contains(&service_event.executable) && is_stop
    }

    fn print_error(device_name: &str, message: &str) {
        red_ln!("{} --> {}", device_name, message)
    }
}