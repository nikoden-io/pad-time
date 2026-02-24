Feature: View Sites List

  Scenario: Admin views sites
    Given I navigate to "/admin/sites"
    Then I should see "Sites Management" heading
    And I should see a table with sites
