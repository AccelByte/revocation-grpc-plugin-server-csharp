// Copyright (c) 2023-2024 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AccelByte.Sdk.Api;
using AccelByte.Sdk.Core;
using AccelByte.Sdk.Core.Util;
using AccelByte.Sdk.Api.Platform.Wrapper;
using AccelByte.Sdk.Api.Platform.Model;
using AccelByte.Sdk.Api.Iam.Model;

using AccelByte.PluginArch.Revocation.Demo.Client.Model;

namespace AccelByte.PluginArch.Revocation.Demo.Client
{
    public class PlatformWrapper
    {
        public const string AB_STORE_NAME = "Custom Revocation Plugin Demo Store";

        public const string AB_STORE_DESC = "Description for custom revocation grpc plugin demo store";

        public const string AB_VIEW_NAME = "Revocation Default View";

        public const string AB_CURRENCY_CODE = "USV";

        public const string AB_CURRENCY_NAME = "Virtual US Dollars";

        private AccelByteSDK _Sdk;

        private ApplicationConfig _Config;

        private string _StoreId = "";

        private string _ViewId = "";

        public PlatformWrapper(ApplicationConfig config)
        {
            _Config = config;
            _Sdk = AccelByteSDK.Builder
                .SetConfigRepository(_Config)
                .UseDefaultHttpClient()
                .UseDefaultTokenRepository()
                .Build();
        }

        public string GetAccessToken()
        {
            return _Sdk.Configuration.TokenRepository.Token;
        }

        public void ConfigureGrpcTargetUrl()
        {
            if (_Config.GrpcServerUrl != "")
            {
                _Sdk.Platform.ServicePluginConfig.UpdateRevocationPluginConfigOp
                    .SetBody(new RevocationPluginConfigUpdate()
                    {
                        ExtendType = RevocationPluginConfigUpdateExtendType.CUSTOM,
                        CustomConfig = new BaseCustomConfig()
                        {
                            ConnectionType = BaseCustomConfigConnectionType.INSECURE,
                            GrpcServerAddress = _Config.GrpcServerUrl
                        }
                    })
                    .Execute(_Sdk.Namespace);
            }
            else if (_Config.ExtendAppName != "")
            {
                _Sdk.Platform.ServicePluginConfig.UpdateRevocationPluginConfigOp
                    .SetBody(new RevocationPluginConfigUpdate()
                    {
                        ExtendType = RevocationPluginConfigUpdateExtendType.APP,
                        AppConfig = new AppConfig()
                        {
                            AppName = _Config.ExtendAppName
                        }
                    })
                    .Execute(_Sdk.Namespace);
            }
            else
                throw new Exception("No Grpc target url configured.");
        }

        public void DeleteGrpcTargetUrl()
        {
            _Sdk.Platform.ServicePluginConfig.DeleteRevocationPluginConfigOp
                .Execute(_Sdk.Namespace);
        }

        public void PublishStoreChange(string storeId)
        {
            try
            {
                _Sdk.Platform.CatalogChanges.PublishAllOp
                    .Execute(_Sdk.Namespace, storeId);
            }
            catch (Exception x)
            {
                Console.WriteLine("PublishStoreChange failed. {0}", x.Message);
                throw;
            }
        }

        public void PublishStoreChange()
            => PublishStoreChange(_StoreId);

        public string CreateStore()
        {
            try
            {
                var stores = _Sdk.Platform.Store.ListStoresOp.Execute(_Sdk.Namespace);
                if (stores == null)
                    stores = new List<StoreInfo>();

                //delete existing draft store(s)
                foreach (var store in stores)
                {
                    if (store.Published.HasValue && !store.Published.Value)
                        _Sdk.Platform.Store.DeleteStoreOp
                            .Execute(_Sdk.Namespace, store.StoreId!);
                }

                //create new draft store
                var newStore = _Sdk.Platform.Store.CreateStoreOp
                    .SetBody(new StoreCreate()
                    {
                        Title = AB_STORE_NAME,
                        Description = AB_STORE_DESC,
                        DefaultLanguage = "en",
                        DefaultRegion = "US",
                        SupportedLanguages = new List<string>() { "en" },
                        SupportedRegions = new List<string>() { "US" }
                    })
                    .Execute(_Sdk.Namespace);
                if (newStore == null)
                    throw new Exception("Could not create new store.");
                _StoreId = newStore.StoreId!;

                return _StoreId;
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateStore failed. {0}", x.Message);
                throw;
            }
        }

        public void CreateCategory(string categoryPath)
        {
            try
            {
                if (_StoreId == "")
                    throw new Exception("No store id stored.");

                _Sdk.Platform.Category.CreateCategoryOp
                    .SetBody(new CategoryCreate()
                    {
                        CategoryPath = categoryPath,
                        LocalizationDisplayNames = new Dictionary<string, string>() { { "en", categoryPath } }
                    })
                    .Execute(_Sdk.Namespace, _StoreId);
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateCategory failed. {0}", x.Message);
                throw;
            }
        }

        public string CreateStoreView()
        {
            try
            {
                if (_StoreId == "")
                    throw new Exception("No store id stored.");

                var newView = _Sdk.Platform.View.CreateViewOp
                    .SetBody(new ViewCreate()
                    {
                        Name = AB_VIEW_NAME,
                        DisplayOrder = 1,
                        Localizations = new Dictionary<string, Localization>()
                        {
                            { "en", new Localization()
                                {
                                    Title = AB_VIEW_NAME
                                }
                            }
                        }
                    })
                    .Execute(_Sdk.Namespace, _StoreId);
                if (newView == null)
                    throw new Exception("Could not create a new store view.");

                _ViewId = newView.ViewId!;
                return _ViewId;
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateStoreView failed. {0}", x.Message);
                throw;
            }
        }
        
        public List<SimpleItemInfo> CreateItems(int itemCount, string categoryPath, string itemDiff)
        {
            try
            {
                if (_StoreId == "")
                    throw new Exception("No store id stored.");

                List<SimpleItemInfo> nItems = new List<SimpleItemInfo>();
                for (int i = 0; i < itemCount; i++)
                {
                    SimpleItemInfo nItemInfo = new SimpleItemInfo();
                    nItemInfo.Title = $"Item {itemDiff} Titled {i + 1}";
                    nItemInfo.Sku = $"SKU_{itemDiff}_{i + 1}";
                    nItemInfo.Price = (i + 1) * 2;

                    var newItem = _Sdk.Platform.Item.CreateItemOp
                        .SetBody(new ItemCreate()
                        {
                            Name = nItemInfo.Title,
                            ItemType = ItemCreateItemType.INGAMEITEM,
                            CategoryPath = categoryPath,
                            EntitlementType = ItemCreateEntitlementType.DURABLE,
                            SeasonType = ItemCreateSeasonType.TIER,
                            Status = ItemCreateStatus.ACTIVE,
                            Listable = true,
                            Purchasable = true,
                            Sku = nItemInfo.Sku,
                            UseCount = 1,
                            Localizations = new Dictionary<string, Localization>()
                            {
                                { "en", new Localization()
                                    {
                                        Title = nItemInfo.Title
                                    }
                                }
                            },
                            RegionData = new Dictionary<string, List<RegionDataItemDTO>>()
                            {
                                { "US", new List<RegionDataItemDTO>()
                                    {
                                        { new RegionDataItemDTO() {
                                            CurrencyCode = AB_CURRENCY_CODE,
                                            CurrencyNamespace = _Sdk.Namespace,
                                            CurrencyType = RegionDataItemDTOCurrencyType.VIRTUAL,
                                            Price = nItemInfo.Price
                                        }}
                                    }
                                }
                            }
                        })
                        .Execute(_Sdk.Namespace, _StoreId);
                    if (newItem == null)
                        throw new Exception("Could not create store item.");

                    nItemInfo.Id = newItem.ItemId!;
                    nItems.Add(nItemInfo);
                }

                return nItems;
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateItems failed. {0}", x.Message);
                throw;
            }
        }

        public List<SimpleItemInfo> CreateItems(int itemCount, string categoryPath)
        {
            string itemDiff = Helper.GenerateRandomId(6).ToUpper();
            return CreateItems(itemCount, categoryPath, itemDiff);
        }

        public List<SimpleLootboxItem> CreateLootboxItems(int itemCount, int rewardItemCount, string categoryPath)
        {
            try
            {
                if (_StoreId == "")
                    throw new Exception("No store id stored.");

                string lbDiff = Helper.GenerateRandomId(6).ToUpper();

                List<SimpleLootboxItem> nItems = new List<SimpleLootboxItem>();
                for (int i = 0; i < itemCount; i++)
                {
                    SimpleLootboxItem nItemInfo = new SimpleLootboxItem();
                    nItemInfo.Title = $"Lootbox Item {lbDiff} Titled {i + 1}";
                    nItemInfo.Sku = $"SKUCL_{lbDiff}_{i + 1}";
                    nItemInfo.Diff = lbDiff;
                    nItemInfo.Price = (i + 1) * 2;

                    List<LootBoxReward> lbRewards = new List<LootBoxReward>();
                    List<SimpleItemInfo> rewardItems = new List<SimpleItemInfo>();
                    for (int j = 1; j <= rewardItemCount; j++)
                    {
                        string itemDiff = Helper.GenerateRandomId(6).ToUpper();
                        List<SimpleItemInfo> items = CreateItems(1, categoryPath, itemDiff);

                        List<BoxItem> rewardBoxItems = new List<BoxItem>();
                        foreach (var item in items)
                        {
                            rewardBoxItems.Add(new BoxItem()
                            {
                                ItemId = item.Id,
                                ItemSku = item.Sku,
                                Count = 1
                            });
                            rewardItems.Add(item);
                        }

                        lbRewards.Add(new LootBoxReward()
                        {
                            Name = $"Reward-{itemDiff}",
                            Odds = 0.1,
                            Weight = 10,
                            Type = LootBoxRewardType.REWARD,
                            LootBoxItems = rewardBoxItems
                        });
                    }
                    nItemInfo.RewardItems = rewardItems;

                    var newItem = _Sdk.Platform.Item.CreateItemOp
                        .SetBody(new ItemCreate()
                        {
                            Name = nItemInfo.Title,
                            ItemType = ItemCreateItemType.LOOTBOX,
                            CategoryPath = categoryPath,
                            EntitlementType = ItemCreateEntitlementType.CONSUMABLE,
                            SeasonType = ItemCreateSeasonType.TIER,
                            Status = ItemCreateStatus.ACTIVE,
                            UseCount = 100,
                            Listable = true,
                            Purchasable = true,
                            Sku = nItemInfo.Sku,
                            LootBoxConfig = new LootBoxConfig()
                            {
                                RewardCount = rewardItemCount,
                                Rewards = lbRewards,
                                RollFunction = LootBoxConfigRollFunction.CUSTOM
                            },
                            Localizations = new Dictionary<string, Localization>()
                            {
                                { "en", new Localization()
                                    {
                                        Title = nItemInfo.Title
                                    }
                                }
                            },
                            RegionData = new Dictionary<string, List<RegionDataItemDTO>>()
                            {
                                { "US", new List<RegionDataItemDTO>()
                                    {
                                        { new RegionDataItemDTO() {
                                            CurrencyCode = AB_CURRENCY_CODE,
                                            CurrencyNamespace = _Sdk.Namespace,
                                            CurrencyType = RegionDataItemDTOCurrencyType.VIRTUAL,
                                            Price = nItemInfo.Price
                                        }}
                                    }
                                }
                            }
                        })
                        .Execute(_Sdk.Namespace, _StoreId);
                    if (newItem == null)
                        throw new Exception("Could not create store lootbox item.");

                    nItemInfo.Id = newItem.ItemId!;
                    nItems.Add(nItemInfo);
                }

                return nItems;
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateLootboxItems failed. {0}", x.Message);
                throw;
            }
        }

        public void DeleteLootboxItems(List<SimpleLootboxItem> lootboxes)
        {
            try
            {
                foreach (var lootbox in lootboxes)
                {
                    foreach (var reward in lootbox.RewardItems)
                    {
                        _Sdk.Platform.Item.DeleteItemOp
                            .SetForce(true)
                            .Execute(reward.Id, _Sdk.Namespace);
                    }
                    _Sdk.Platform.Item.DeleteItemOp
                        .SetForce(true)
                        .Execute(lootbox.Id, _Sdk.Namespace);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("DeleteLootboxItems failed. {0}", x.Message);
                throw;
            }
        }

        public void DeleteItems(List<SimpleItemInfo> items)
        {
            try
            {
                foreach (var item in items)
                {
                    _Sdk.Platform.Item.DeleteItemOp
                        .SetForce(true)
                        .Execute(item.Id, _Sdk.Namespace);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("DeleteItems failed. {0}", x.Message);
                throw;
            }
        }

        public SimpleSectionInfo CreateSectionWithItems(int itemCount, string categoryPath)
        {
            try
            {
                if (_StoreId == "")
                    throw new Exception("No store id stored.");
                if (_ViewId == "")
                    throw new Exception("No view id stored.");

                string itemDiff = Helper.GenerateRandomId(6).ToUpper();
                List<SimpleItemInfo> items = CreateItems(itemCount, categoryPath, itemDiff);

                List<SectionItem> sectionItems = new List<SectionItem>();
                foreach (var item in items)
                    sectionItems.Add(new SectionItem()
                    {
                        Id = item.Id,
                        Sku = item.Sku
                    });

                string sectionTitle = $"{itemDiff} Section";

                var newSection = _Sdk.Platform.Section.CreateSectionOp
                    .SetBody(new SectionCreate()
                    {
                        ViewId = _ViewId,
                        DisplayOrder = 1,
                        Name = sectionTitle,
                        Active = true,
                        StartDate = DateTime.Now.AddDays(-1),
                        EndDate = DateTime.Now.AddDays(1),
                        RotationType = SectionCreateRotationType.FIXEDPERIOD,
                        FixedPeriodRotationConfig = new FixedPeriodRotationConfig()
                        {
                            BackfillType = FixedPeriodRotationConfigBackfillType.NONE,
                            Rule = FixedPeriodRotationConfigRule.SEQUENCE
                        },
                        Localizations = new Dictionary<string, Localization>()
                        {
                            { "en", new Localization()
                                {
                                    Title = sectionTitle
                                }
                            }
                        },
                        Items = sectionItems
                    })
                    .Execute(_Sdk.Namespace, _StoreId);

                if (newSection == null)
                    throw new Exception("Could not create new store section.");

                SimpleSectionInfo result = new SimpleSectionInfo()
                {
                    Id = newSection.SectionId!,
                    Items = items
                };

                return result;
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateSectionWithItems failed. {0}", x.Message);
                throw;
            }
        }

        public string GrantEntitlement(string userId, string itemId, int count)
        {
            try
            {
                if (_StoreId == "")
                    throw new Exception("No store id stored.");

                List<EntitlementGrant> eGrants = new List<EntitlementGrant>
                {
                    new EntitlementGrant()
                    {
                        ItemId = itemId,
                        Quantity = count,
                        Source = EntitlementGrantSource.GIFT,
                        StoreId = _StoreId,
                        ItemNamespace = _Sdk.Namespace
                    }
                };

                var eInfo = _Sdk.Platform.Entitlement.GrantUserEntitlementOp
                    .SetBody(eGrants)
                    .Execute(_Sdk.Namespace, userId);
                if (eInfo == null)
                    throw new Exception("Could not grant user entitlement.");

                if (eInfo.Count <= 0)
                    throw new Exception("Could not grant item to user");

                return eInfo[0].Id!;
            }
            catch (Exception x)
            {
                Console.WriteLine("GrantEntitlement failed. {0}", x.Message);
                throw;
            }
        }

        public SimpleLootboxItem ConsumeItemEntitlement(string userId, string entitlementId, int useCount)
        {
            try
            {
                var result = _Sdk.Platform.Entitlement.ConsumeUserEntitlementOp
                    .SetBody(new AdminEntitlementDecrement()
                    {
                        UseCount = useCount
                    })
                    .Execute(entitlementId, _Sdk.Namespace, userId);
                if (result == null)
                    throw new Exception("Could not consume user entitlement.");

                if (result.Rewards == null)
                    throw new Exception("Result does not contains Rewards field.");

                List<SimpleItemInfo> items = new List<SimpleItemInfo>();
                foreach (var reward in result.Rewards)
                {
                    items.Add(new SimpleItemInfo()
                    {
                        Id = reward.ItemId!,
                        Sku = reward.ItemSku!
                    });
                }

                return new SimpleLootboxItem()
                {
                    Id = result.ItemId!,
                    RewardItems = items
                };
            }
            catch (Exception x)
            {
                Console.WriteLine("ConsumeItemEntitlement failed. {0}", x.Message);
                throw;
            }
        }

        public void DeleteStore(string storeId)
        {
            try
            {
                _Sdk.Platform.Store.DeleteStoreOp
                    .Execute(_Sdk.Namespace, storeId);
            }
            catch (Exception x)
            {
                Console.WriteLine("DeleteStore failed. {0}", x.Message);
                throw;
            }
        }

        public void DeleteStore()
        {
            if (_StoreId == "")
                throw new Exception("No store id stored.");
            DeleteStore(_StoreId);
        }

        public string CreateOrder(string userId, SimpleItemInfo item, int quantity)
        {
            try
            {
                OrderInfo? order = _Sdk.Platform.Order.AdminCreateUserOrderOp
                    .SetBody(new AdminOrderCreate()
                    {
                        CurrencyCode = AB_CURRENCY_CODE,
                        ItemId = item.Id,
                        Price = quantity * item.Price,
                        Quantity = quantity,
                        DiscountedPrice = quantity * item.Price,
                        Region = "US"
                    })
                    .Execute(_Sdk.Namespace, userId);
                if (order == null)
                    throw new Exception("Could create an order.");
                return order.OrderNo!;
            }
            catch (Exception x)
            {
                Console.WriteLine("CreateOrder failed. {0}", x.Message);
                throw;
            }
        }

        public ItemRevocationResult RevokeOrder(string userId, string orderNo, SimpleItemInfo item)
        {
            try
            {
                RevocationResult? result = _Sdk.Platform.Revocation.DoRevocationOp
                    .SetBody(new RevocationRequest()
                    {
                        Source = RevocationRequestSource.ORDER,
                        TransactionId = orderNo,
                        RevokeEntries = new List<RevokeEntry>()
                        {
                            new RevokeEntry()
                            {
                                Quantity = 1,
                                Type = RevokeEntryType.ITEM,
                                Item = new RevokeItem()
                                {
                                    ItemIdentity = item.Id,
                                    ItemIdentityType = RevokeItemItemIdentityType.ITEMID
                                }
                            }
                        }
                    })
                    .Execute(_Sdk.Namespace, userId);
                if (result == null)
                    throw new Exception("Could not revoke order.");

                return new ItemRevocationResult(result);
            }
            catch (Exception x)
            {
                Console.WriteLine("RevokeOrder failed. {0}", x.Message);
                throw;
            }
        }

        public void Login()
        {
            bool loginResult = _Sdk.LoginClient();
            if (!loginResult)
                throw new Exception("Login failed!");
        }

        public AccountCreateUserResponseV4 CreateTestUser()
        {
            string nameId = Helper.GenerateRandomId(8);
            string userName = $"extend_{nameId}_user";
            string dName = $"Extend Test User {nameId}";

            var newUser = _Sdk.Iam.UsersV4.PublicCreateTestUserV4Op
                .Execute(new AccountCreateTestUserRequestV4()
                {
                    Verified = true,
                    EmailAddress = $"{userName}@dummy.net",
                    Username = userName,
                    DisplayName = dName,
                    UniqueDisplayName = dName,
                    Country = "ID",
                    AcceptedPolicies = new List<LegalAcceptedPoliciesRequest>(),
                    AuthType = AccountCreateTestUserRequestV4AuthType.EMAILPASSWD,
                    DateOfBirth = "1990-01-01",
                    Password = Helper.GenerateRandomPassword(16)
                }, _Sdk.Namespace);
            if (newUser == null)
                throw new Exception("Could not create user");

            return newUser;
        }

        public void DeleteUser(string userId)
        {
            _Sdk.Iam.Users.AdminDeleteUserInformationV3Op
                .Execute(_Sdk.Namespace, userId);
        }

        public void Logout()
        {
            _Sdk.Logout();
        }

        public void CheckAndCreateCurrencyIfNotExists()
        {
            try
            {
                var currencies = _Sdk.Platform.Currency.ListCurrenciesOp
                    .Execute(_Sdk.Namespace);
                if (currencies == null)
                    throw new Exception("Could not retrieve list of currencies.");

                bool isCurrencyFound = false;
                foreach (var currencyItem in currencies)
                {
                    if (currencyItem.CurrencyCode == AB_CURRENCY_CODE)
                    {
                        isCurrencyFound = true;
                        break;
                    }
                }

                if (!isCurrencyFound)
                {
                    _Sdk.Platform.Currency.CreateCurrencyOp
                        .SetBody(new CurrencyCreate()
                        {
                            CurrencyCode = AB_CURRENCY_CODE,
                            CurrencySymbol = AB_CURRENCY_CODE,
                            CurrencyType = CurrencyCreateCurrencyType.VIRTUAL,
                            Decimals = 0,
                            LocalizationDescriptions = new Dictionary<string, string>()
                            {
                                { "en", AB_CURRENCY_NAME }
                            }
                        })
                        .Execute(_Sdk.Namespace);
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("CheckAndCreateCurrencyIfNotExists failed. {0}", x.Message);
                throw;
            }
        }

        public void CheckUserWalletBalance(string userId)
        {
            try
            {
                var checkResult = _Sdk.Platform.Wallet.QueryUserCurrencyWalletsOp
                    .Execute(_Sdk.Namespace, userId);
                if (checkResult == null)
                    throw new Exception("Could not retrieve wallet summary.");

                CurrencyWallet? currencyWallet = null;
                foreach (var wallet in checkResult)
                {
                    if (wallet.CurrencyCode! == AB_CURRENCY_CODE)
                    {
                        currencyWallet = wallet;
                        break;
                    }
                }

                if (currencyWallet == null)
                    throw new Exception($"Wallet for {AB_CURRENCY_CODE} does not exists.");

                if (currencyWallet.Balance!.Value < 500)
                {
                    //not enough balance, so fill it.
                    var addResult = _Sdk.Platform.Wallet.CreditUserWalletOp
                        .SetBody(new CreditRequest()
                        {
                            Amount = 500,
                            Origin = CreditRequestOrigin.Other,
                            Source = CreditRequestSource.OTHER,
                            Reason = "Grpc Plugin Test"
                        })
                        .Execute(AB_CURRENCY_CODE, _Sdk.Namespace, userId);
                    if (addResult == null)
                        throw new Exception("Could not credit user's wallet.");

                    if (addResult.Balance!.Value < 500)
                        throw new Exception("Failed to credit user's wallet.");
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("GetUserWalletBalance failed. {0}", x.Message);
                throw;
            }
        }
    }
}
