﻿using LightBDD.Formatting.Parameters;
using MbUnit.Framework;

namespace LightBDD.Example.AcceptanceTests.MbUnit.Features
{
    public partial class Invoice_feature : FeatureFixture
    {
        private void Product_is_available_in_product_storage(string product)
        {
        }

        private void Customer_buys_product(string product)
        {
        }

        private void An_invoice_should_be_sent_to_the_customer()
        {
            Assert.Inconclusive("Not implemented yet");
        }

        private void The_invoice_should_contain_product_with_price_of_AMOUNT(string product, [Format("{0:£0.00#}")]decimal amount)
        {
        }
    }
}