package watchdog

import Config
import api.*
import kotlinx.serialization.ExperimentalSerializationApi
import watchdog.notifications.NotificationStrategy

@ExperimentalSerializationApi
class Watchdog(private val config: Config, private val notificationStrategy: NotificationStrategy) {
    fun onNewValue(deviceName: String, systemValue: SystemValue) {
        when (systemValue) {
            is CpuLoad ->
                if (checkCpuLoad(systemValue)) notificationStrategy.notify(deviceName, cpuLoadMessage(systemValue))
            is RamUsage ->
                if (checkRamUsage(systemValue)) notificationStrategy.notify(deviceName, ramUsageMessage(systemValue))
            is ServiceEvent ->
                if (checkServiceEvent(systemValue)) notificationStrategy.notify(
                    deviceName,
                    serviceEventMessage(systemValue)
                )
        }
    }

    private fun checkCpuLoad(cpuLoad: CpuLoad): Boolean = cpuLoad.loadPercentage > config.cpuMax

    private fun checkRamUsage(ramUsage: RamUsage) = 100 * ramUsage.usedMb / ramUsage.totalMb > config.ramMax

    private fun checkServiceEvent(serviceEvent: ServiceEvent) =
        serviceEvent.serviceEventType == ServiceEventType.Stop
                && config.delicateProcesses.contains(serviceEvent.executable)

    private fun cpuLoadMessage(cpuLoad: CpuLoad) =
        "Cpu load was ${cpuLoad.loadPercentage}% (Threshold is ${config.cpuMax}%)"

    private fun ramUsageMessage(ramUsage: RamUsage) =
        "Ram usage was ${ramUsage.usedMb} out of ${ramUsage.totalMb} (Threshold is ${config.ramMax}%)"

    private fun serviceEventMessage(serviceEvent: ServiceEvent) =
        "Critical service ${serviceEvent.executable} was stopped"
}