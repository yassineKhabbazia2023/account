# Variables d'environnement

| Projet | Variable | Valeur | Description |
|---|---|---|---|
| Account.API | APPINSIGHTS_INSTRUMENTATIONKEY | ef648e6e-8440-4545-a001-0e71b1fa59e2 | Clé AppInsights |
| Account.API | APPLICATIONINSIGHTS_CONNECTION_STRING | InstrumentationKey=ef648e6e-8440-4545-a001-0e71b1fa59e2;IngestionEndpoint=https://francecentral-1.in.applicationinsights.azure.com/;LiveEndpoint=https://francecentral.livediagnostics.monitor.azure.com/;ApplicationId=58c023e6-78f4-497a-910a-1bb15a7462f8 | Chaîne de connexion AppInsights |
| Account.API | BrokerSetting__ManagedIdentityClientId | 9c2a2e09-0915-4427-910f-25b51b49d257 | ID de l'identité managée du broker |
| Account.API | BrokerSetting__ServiceBusNamespace | sbnscegpulsehubrec0101.servicebus.windows.net | Namespace du Service Bus |
| Account.API | BrokerSetting__PullTopics__0__TopicName | contact | Nom du topic contact |
| Account.API | BrokerSetting__PullTopics__0__Subscriptions__0 | contact-created-account | Subscription contactCreated sur le topic contact |
| Account.API | BrokerSetting__PullTopics__0__Subscriptions__1 | contact-updated-account | Subscription contactUpdated sur le topic contact |
| Account.API | BrokerSetting__PullTopics__0__Subscriptions__2 | contact-removed-account | Subscription contactRemoved sur le topic contact |
| Account.API | BrokerSetting__PullTopics__1__TopicName | registry | Nom du topic registry |
| Account.API | BrokerSetting__PullTopics__1__Subscriptions__0 | registry-accountCreated-account | Subscription accountCreated sur le topic registry |
| Account.API | BrokerSetting__PullTopics__1__Subscriptions__1 | registry-accountUpdated-account | Subscription accountUpdated sur le topic registry |
| Account.API | BrokerSetting__PullTopics__1__Subscriptions__2 | registry-accountRemoved-account | Subscription accountRemoved sur le topic registry |
| Account.API | BrokerSetting__PullTopics__1__Subscriptions__3 | registry-roleCreated-account | Subscription roleCreated sur le topic registry |
| Account.API | BrokerSetting__PullTopics__1__Subscriptions__4 | registry-roleRemoved-account | Subscription roleRemoved sur le topic registry |
| Account.API | BrokerSetting__PullTopics__2__TopicName | account | Nom du topic account |
| Account.API | BrokerSetting__PullTopics__2__Subscriptions__0 | role-created-account | Subscription roleCreated sur le topic account |
| Account.API | BrokerSetting__PushTopicName__0 | account | Nom du topic push account |
| Account.API | SqlAccountConnectionString | Server=tcp:sqlcegpulseaccrec0101.database.windows.net,1433;Initial Catalog=sqldbcegpulseaccrec0101;Encrypt=True;Authentication=Active Directory Managed Identity;User Id=9c2a2e09-0915-4427-910f-25b51b49d257 | Chaîne de connexion SQL Account |