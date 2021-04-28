pub mod system_value;

pub mod types {
    include!(concat!(env!("OUT_DIR"), "/system_values.rs"));
}