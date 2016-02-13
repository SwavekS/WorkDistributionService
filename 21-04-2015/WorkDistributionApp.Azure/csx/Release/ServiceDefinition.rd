<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="WorkDistributionApp.Azure" generation="1" functional="0" release="0" Id="534442ac-5f8a-4e5a-a995-c97313124d98" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="WorkDistributionApp.AzureGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="WorkDistributionApp:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/LB:WorkDistributionApp:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="WorkDistributionAppInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/MapWorkDistributionAppInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:WorkDistributionApp:Endpoint1">
          <toPorts>
            <inPortMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/WorkDistributionApp/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapWorkDistributionAppInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/WorkDistributionAppInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="WorkDistributionApp" generation="1" functional="0" release="0" software="C:\Users\Administrator\Desktop\21-04-2015\WorkDistributionApp.Azure\csx\Release\roles\WorkDistributionApp" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="-1" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;WorkDistributionApp&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;WorkDistributionApp&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/WorkDistributionAppInstances" />
            <sCSPolicyUpdateDomainMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/WorkDistributionAppUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/WorkDistributionAppFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="WorkDistributionAppUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="WorkDistributionAppFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="WorkDistributionAppInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="1c70e5c1-6121-4fa0-a088-ad238a0a1770" ref="Microsoft.RedDog.Contract\ServiceContract\WorkDistributionApp.AzureContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="ee6c5d89-ce17-48ed-b11d-0269d119decc" ref="Microsoft.RedDog.Contract\Interface\WorkDistributionApp:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/WorkDistributionApp.Azure/WorkDistributionApp.AzureGroup/WorkDistributionApp:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>